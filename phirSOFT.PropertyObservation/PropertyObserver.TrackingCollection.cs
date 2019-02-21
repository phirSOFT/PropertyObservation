using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

#nullable enable
namespace phirSOFT.PropertyObservation
{
    public partial class PropertyObserver<TObject> : IDisposable where TObject : class, INotifyPropertyChanged
    {
        private class TrackingCollection : ICollection<TObject>
        {
            private readonly PropertyObserver<TObject> _observer;

            public TrackingCollection(PropertyObserver<TObject> propertyObserver)
            {
                _observer = propertyObserver;
            }

            public int Count => _observer._trackedInstances.Count;

            public bool IsReadOnly => false;

            public void Add(TObject item)
            {
                if (!_observer.TrackedInstances.Contains(item))
                {
                    item.PropertyChanged += _observer.InstancePropertyChanged;
                }
                _observer._trackedInstances.Add(item);
            }

            public void Clear()
            {
                foreach (var item in _observer.TrackedInstances)
                {
                    item.PropertyChanged -= _observer.InstancePropertyChanged;
                }
                _observer._trackedInstances.Clear();
            }

            public bool Contains(TObject item)
            {
                return _observer._trackedInstances.Contains(item);
            }

            public void CopyTo(TObject[] array, int arrayIndex)
            {
                _observer._trackedInstances.CopyTo(array, arrayIndex);
            }

            public IEnumerator<TObject> GetEnumerator()
            {
                return _observer._trackedInstances.GetEnumerator();
            }

            public bool Remove(TObject item)
            {
                if(_observer.TrackedInstances.Contains(item))
                {
                    item.PropertyChanged -= _observer.InstancePropertyChanged;
                    return _observer._trackedInstances.Remove(item);
                }
                return false;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _observer._trackedInstances.GetEnumerator();
            }
        }
    }



}
