using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace PropertyObservation.Test.Mocks
{
    class ObservableMock<T> : INotifyPropertyChanged
    {
        private T property;
        private T readOnlyProperty;
        private T writeOnlyProperty;
        private T otherProperty;

        public event PropertyChangedEventHandler PropertyChanged;

        public T Property { get => property; set => SetProperty(ref property, value); }

        public T ReadOnlyProperty { get => readOnlyProperty; private set => SetProperty(ref readOnlyProperty, value) ;}

        public void SetReadOnlyValue(T value)
        {
            readOnlyProperty = value;
        }

        public T WriteOnlyProperty { private get => writeOnlyProperty; set => SetProperty(ref writeOnlyProperty, value); }

        public T GetWriteOnlyProperty()
        {
            return WriteOnlyProperty;
        }

        public T OtherProperty { get => otherProperty; set => SetProperty(ref otherProperty, value); }

        private void SetProperty(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            storage = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
