using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Smart.Bindings
{
    /// <summary>
    /// Read-only version SmartProperty.
    /// </summary>
    public interface IReadOnlySmartProperty : INotifyPropertyChanged
    {
        object Value { get; }
    }

    /// <summary>
    /// Type safed read-only version SmartProperty.
    /// </summary>
    public interface IReadOnlySmartProperty<T> : IReadOnlySmartProperty
    {
        new T Value { get; }
    }
}
