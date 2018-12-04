using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Smart.Bindings
{
    /// <summary>
    /// SmartProperty interface.
    /// </summary>
    public interface ISmartProperty : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        object Value { get; set; }
    }

    /// <summary>
    /// Type safed SmartProperty.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISmartProperty<T> : ISmartProperty
    {
        new T Value { get; set; }
    }
}

