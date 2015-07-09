using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Inzynierka.ViewModel
{
    public class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        #region Constructor

        protected ViewModelBase()
        {

        }
        #endregion // Constructor
        #region Debugging Aides

        ///

        /// Warns the developer if this object does not have a public property
        /// with the specified name. This method does not exist in a Release build
        /// 

        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real,
            // public, instance property on this object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = "Invalid property name: " + propertyName;

                if (this.ThrowInvalidPropertyName)
                    throw new Exception(msg);
                else
                {
                    Debug.Fail(msg);
                }
            }
        }
        public virtual bool ThrowInvalidPropertyName { get; set; }
        #endregion // Debugging Aides

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName)
        {
            this.VerifyPropertyName(propertyName);
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }
        #endregion // INotifyPropertyChanged Members
        #region IDisposable

        public void Dispose()
        {
            this.onDispose();
        }

        protected virtual void onDispose()
        {
        }
        #endregion // IDisposable Members
    }
}