using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Smart.Bindings
{
    public class SmartProperty<T> : ISmartProperty<T>
    {
        private static PropertyChangedEventArgs ValuePropertyChangedEventArgs { get; } = new PropertyChangedEventArgs(nameof(Value));
        private static DataErrorsChangedEventArgs DataErrorsChangedEventArgs { get; } = new DataErrorsChangedEventArgs(nameof(Value));

        protected Func<T, IEnumerable> ValidationLogic { get; set; }
        private IEqualityComparer<T> EqualityComparer { get; }
        private bool IsDistinctUntilChanged { get; }
        protected IEnumerable Errors { get; set; }
        protected T LatestValue { get; set; }

        public SmartProperty(T initialValue, bool isDistinctUntilChanged)
        {
            EqualityComparer = EqualityComparer<T>.Default;
            LatestValue = initialValue;
            IsDistinctUntilChanged = isDistinctUntilChanged;
        }

        public SmartProperty(T initialValue) : this(initialValue, true)
        {
        }

        public SmartProperty() : this(default(T), true)
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public T Value
        {
            get => LatestValue;
            set
            {
                if (SetValue(value))
                {
                    ValidateValue();
                }
            }
        }

        object ISmartProperty.Value
        {
            get => Value;
            set => Value = (T)value;
        }

        public bool HasErrors => Errors != null;

        protected virtual bool SetValue(T value)
        {
            if (IsDistinctUntilChanged && EqualityComparer.Equals(LatestValue, value))
            {
                return false;
            }

            LatestValue = value;
            PropertyChanged?.Invoke(this, ValuePropertyChangedEventArgs);
            return true;
        }

        protected virtual void ValidateValue()
        {
            Errors = ValidationLogic?.Invoke(LatestValue) ?? null;
            ErrorsChanged?.Invoke(this, DataErrorsChangedEventArgs); 
        }

        public IEnumerable GetErrors(string propertyName)
        {
            return Errors;
        }

        public SmartProperty<T> SetValidateNotifyError(Func<T, IEnumerable> validationLogic)
        {
            ValidationLogic = validationLogic;
            return this;
        }

        public SmartProperty<T> SetValidateNotifyError(Func<T, string> validationLogic)
        {
            ValidationLogic = (T value) =>
            {
                var errorResult = validationLogic(value);
                if (string.IsNullOrEmpty(errorResult))
                {
                    return null;
                }

                return new[] { errorResult };
            };

            return this;
        }

        public SmartProperty<T> SetValidateAttribute(Expression<Func<SmartProperty<T>>> selfSelector)
        {
            var memberExpression = (MemberExpression)selfSelector.Body;
            var propertyInfo = (PropertyInfo)memberExpression.Member;
            var attrs = propertyInfo.GetCustomAttributes<ValidationAttribute>().ToArray();
            var context = new ValidationContext(this)
            {
                MemberName = nameof(Value)
            };

            if (attrs.Length != 0)
            {
                SetValidateNotifyError(x =>
                {
                    try
                    {
                        Validator.ValidateValue(x, context, attrs);
                        return null;
                    }
                    catch (ValidationException ex)
                    {
                        return ex.ValidationResult.ErrorMessage;
                    }
                });
            }
            return this;
        }
    }
}
