using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Lomont.WPF.MVVM
{

    /// <summary>
    /// Simple class to handle cross system notifications.
    /// Register a message and an action to take, then anyone
    /// else can trigger that action.
    /// Useful for communication between view models, etc.
    /// 
    /// Note: delegates stored using weak references internally
    /// to prevent holding onto objects that get removed.
    /// </summary>
    /// <example>
    /// Use like:
    /// 1. Mediator.Register("Command",action1);
    /// 2. Mediator.NotifyColleagues("Command",argObject);
    /// 3. Mediator.Unregister("Command",action1); // must give same action to remove it
    /// 
    /// TODO
    ///  1. make templated to allow specific action delegates, prevent type loss
    ///  2. make relative to a Dispatcher, and invoke as needed? (or invoke elsewhere)
    ///  3. Unregister removed, made weak references. Reinstate a way to remove items 
    /// </example>
    public static class Mediator
    {
        private static IDictionary<string, List<WeakAction>> mediatorDictionary =
            new Dictionary<string, List<WeakAction>>();

        /// <summary>
        /// Register the string token as calling the callback action 
        /// whenever the token is issued
        /// </summary>
        /// <param name="token"></param>
        /// <param name="callback"></param>
        public static void Register(string token, Action<object> callback)
        {
            if (token == null)
                throw new ArgumentNullException("message");
            if (callback == null)
                throw new ArgumentNullException("callback");

            if (callback.Target == null)
                throw new ArgumentException("The 'callback' delegate must reference an instance method.");

            if (!mediatorDictionary.ContainsKey(token))
            {
                var list = new List<WeakAction> { new WeakAction(callback) };
                mediatorDictionary.Add(token, list);
            }
            else
            {
                // if proper name for the item matches, we already have it
                var actions = GetLiveActions(token);

                var found =
                    actions != null &&
                    actions.Any(item => item.Method.ToString() == callback.Method.ToString());

                if (!found)
                    mediatorDictionary[token].Add(new WeakAction(callback));
            }
        }


        /// <summary>
        /// Remove the token command and callback
        /// </summary>
        /// <param name="token"></param>
        /// <param name="callback"></param>
        //public static void Unregister(string token, Action<object> callback)
        //{
        //if (mediatorDictionary.ContainsKey(token))
        //{
        //    mediatorDictionary[token].Remove(callback);
        //}
        //}

        /// <summary>
        /// Execute command on any listeners
        /// </summary>
        /// <param name="token"></param>
        /// <param name="args"></param>
        public static void NotifyColleagues(string token, object args = null)
        {
            var actions = GetLiveActions(token);
            if (actions != null)
            {
                foreach (var weakReference in mediatorDictionary[token])
                {
                    var action = weakReference.CreateAction();
                    if (action != null)
                        action(args);
                }
            }
        }


        #region Implementation

        /// <summary>
        /// Use this to get the list of live actions.
        /// Cleans out dead ones
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>

        static List<Action<object>> GetLiveActions(string token)
        {
            if (token == null)
                throw new ArgumentNullException("token");

            if (!mediatorDictionary.ContainsKey(token))
                return null;

            var weakActions = mediatorDictionary[token];
            var actions = new List<Action<object>>();
            for (var i = weakActions.Count - 1; i >= 0; --i)
            {
                var weakAction = weakActions[i];
                if (!weakAction.IsAlive)
                    weakActions.RemoveAt(i);
                else
                    actions.Add(weakAction.CreateAction());
            }

            if (weakActions.Count == 0)
                mediatorDictionary.Remove(token);

            return actions;
        }


        /// <summary>
        /// A weak action holds a weak reference to the action
        /// </summary>
        class WeakAction : WeakReference
        {
            readonly MethodInfo method;

            internal WeakAction(Action<object> action) : base(action.Target)
            {
                method = action.Method;
            }

            internal Action<object> CreateAction()
            {
                if (!base.IsAlive)
                    return null;

                try
                {
                    // Rehydrate into a real Action
                    // object, so that the method
                    // can be invoked on the target.
                    // Does not work in silverlight, or windows phone? test them
                    return Delegate.CreateDelegate(
                        typeof(Action<object>),
                        base.Target,
                        method.Name)
                        as Action<object>;
                }
                catch
                {
                    return null;
                }
            }
        }
        #endregion
    }
}
