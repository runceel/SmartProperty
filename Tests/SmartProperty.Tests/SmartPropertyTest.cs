using System;
using Xunit;

namespace Smart.Bindings.Tests
{
    public class SmartPropertyTest
    {
        [Fact]
        public void DefaultConstructorSetNullValue()
        {
            var p = new SmartProperty<string>();
            p.Value.IsNull();
        }

        [Fact]
        public void ConstructorWithDefaultValue()
        {
            var p = new SmartProperty<string>("hello");
            p.Value.Is("hello");
        }

        [Fact]
        public void RaisePropertyChanged()
        {
            bool called = false;
            string propertyName = null;
            var p = new SmartProperty<string>();
            p.PropertyChanged += (_, e) =>
            {
                called = true;
                propertyName = e.PropertyName;
            };

            p.Value = "change";
            called.IsTrue();
            propertyName.Is("Value");
        }

        [Fact]
        public void IsDistinctUntilChangedTrueCase()
        {
            var p = new SmartProperty<string>(null, true);
            var count = 0;

            p.PropertyChanged += (_, __) => count++;

            p.Value = "hello"; // raised
            p.Value = "hello"; // doesn't raise
            count.Is(1);
        }

        [Fact]
        public void IsDistinctUntilChangedFalseCase()
        {
            var p = new SmartProperty<string>(null, false);
            var count = 0;

            p.PropertyChanged += (_, __) => count++;

            p.Value = "hello"; // raised
            p.Value = "hello"; // raised
            count.Is(2);
        }
    }
}
