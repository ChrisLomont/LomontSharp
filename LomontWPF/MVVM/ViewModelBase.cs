using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Lomont.WPF.MVVM
{
    /// <summary>
    /// Create a view model. Assumes this is created on the UI thread
    /// </summary>
    public class ViewModelBase : NotifiableBase
    {

        // a factory that spawns tasks on the UI thread
        readonly TaskFactory uiFactory;

        public ViewModelBase()
        {
            // Construct a TaskFactory that uses the UI thread's context
            uiFactory = new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        /// Dispatch a task onto the UI thread. Optionally make it synchronous
        /// </summary>
        /// <param name="action"></param>
        /// <param name="synchronous"></param>
        public void Dispatch(Action action, bool synchronous = false)
        {
            if (Dispatcher.FromThread(Thread.CurrentThread) != null)
                action(); // we're on a thread with a valid dispatcher
            else
            {
                if (synchronous)
                    uiFactory.StartNew(action).Wait();
                else
                    uiFactory.StartNew(action);
            }
        }

    }
}
