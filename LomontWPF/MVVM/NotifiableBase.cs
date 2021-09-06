using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Lomont.WPF.MVVM
{
    /// <summary>
    /// This class is a base class to models that need to expose Notifiable properties.
    /// This class provides that functionality to all base classes.
    /// Nothing that is not related to providing some level of notifiable properties
    /// should be added to class. Especially variables that are used as a global or static
    /// variable across classes that implement this base class.
    /// </summary>
    public class NotifiableBase : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members
        /// <summary>
        /// The PropertyChanged event is used by consuming code                
        /// (like WPF's binding infrastructure) to detect when                
        /// a value has changed.                
        /// </summary>                
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raise the PropertyChanged event for the specified property.
        /// </summary>
        /// <param name="propertyName">                
        /// A string representing the name of the property that changed.
        /// </param>                
        /// <remarks>               
        /// Only raise the event if the value of the property                 
        /// has changed from its previous value to prevent infinite 
        /// cycles of property changes.
        /// </remarks>
        protected virtual void NotifyPropertyChanged(string propertyName)
        {
            // Validate the property name in debug builds                    
            VerifyProperty(propertyName);
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        /// <summary>
        /// Safely raises the property changed event.
        /// </summary>
        /// <param name="selectorExpression">An expression like ()=>PropName giving the name of the property to raise.</param>
        protected virtual void NotifyPropertyChanged<T>(Expression<Func<T>> selectorExpression)
        {
            if (selectorExpression == null)
                throw new ArgumentNullException(nameof(selectorExpression));
            var body = selectorExpression.Body as MemberExpression;
            if (body == null)
                throw new ArgumentException("The body must be a member expression");
            NotifyPropertyChanged(body.Member.Name);
        }

        /// <summary>       
        /// In DEBUG mode, verifies whether the current class provides the given property.   
        /// </summary>
        /// <remarks>
        /// Verifies whether the current class provides a property with a given               
        /// name. This method is only invoked in debug builds, and results in               
        /// a runtime exception if the <see cref="RaisePropertyChanged"/> method 
        /// is being invoked with an invalid property name. This may happen if  
        /// a property's name was changed but not the parameter of the property's  
        /// invocation of <see cref="RaisePropertyChanged"/>.             
        /// </remarks>      
        /// <param name="propertyName">The name of the changed property.</param>   
        [System.Diagnostics.Conditional("DEBUG")]
        private void VerifyProperty(string propertyName)
        {
            var type = GetType();
            // Look for a *public* property with the specified name     
            var pi = type.GetProperty(propertyName);
            if (pi == null)
            {
                // There is no matching property - notify the developer   
                var msg = "RaisePropertyChanged was invoked with invalid " +
                    "property name {0}. {0} is not a public " +
                    "property of {1}.";
                msg = string.Format(msg, propertyName, type.FullName);
                System.Diagnostics.Debug.Fail(msg);
            }
        }
        /// <summary>
        /// Set a field if it is not already equal. Return true if there was a change.
        /// </summary>
        /// <param name="field">The field backing to update on change</param>
        /// <param name="value">The new value</param>
        /// <param name="selectorExpression">An expression like ()=>PropName giving the name of the property to raise.</param>
        protected bool SetField<T>(ref T field, T value, Expression<Func<T>> selectorExpression)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            NotifyPropertyChanged(selectorExpression);
            return true;
        }

        #endregion
    }
}
