using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Linq;

namespace BingMaps_GPS_WPF.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ValidatableViewModelBase : DisposableViewModelBase, INotifyDataErrorInfo
    {
        public ValidatableViewModelBase()
        {
        }

        #region ValidateProperty

        protected void ValidateProperty(object value, [CallerMemberName]string propertyName = null)
        {
            var context = new ValidationContext(this) { MemberName = propertyName };
            var validationErrors = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            if (!Validator.TryValidateProperty(value, context, validationErrors))
            {
                var errors = validationErrors.Select(error => error.ErrorMessage);
                SetErrors(propertyName, errors);
            }
            else
            {
                ClearErrors(propertyName);
            }
        }

        readonly Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();
        public Dictionary<string, List<string>> Errors
        {
            get { return _errors; }
        }

        protected void SetErrors(string propertyName, IEnumerable<string> errors)
        {
            var existserror = _errors.ContainsKey(propertyName);
            var existsnewerror = errors != null && errors.Count() > 0;

            if (!existserror && !existsnewerror)
                return;

            if (existsnewerror)
            {
                _errors[propertyName] = new List<string>(errors);
            }
            else
            {
                _errors.Remove(propertyName);
            }
        }

        protected void ClearErrors(string propertyName)
        {
            if (_errors.ContainsKey(propertyName))
            {
                _errors.Remove(propertyName);
                OnErrorsChanged(propertyName);
            }
        }

        private void OnErrorsChanged(string propertyName)
        {
            var h = this.ErrorsChanged;
            if (h != null)
            {
                h(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region INotifyDataErrorInfo

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public System.Collections.IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName) ||
                !_errors.ContainsKey(propertyName))
                return null;

            return _errors[propertyName];
        }

        public bool HasErrors
        {
            get { return _errors.Count > 0; }
        }

        #endregion
    }
}