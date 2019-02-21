using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

#nullable enable
namespace phirSOFT.PropertyObservation
{
    public partial class PropertyObserver<TObject> : IDisposable where TObject : class, INotifyPropertyChanged
    {
        private readonly HashSet<TObject> _trackedInstances = new HashSet<TObject>(ReferenceEqualityComparer<TObject>.Comparer);

        public PropertyObserver()
        {
            TrackedInstances = new PropertyObserver<TObject>.TrackingCollection(this);
        }

        private Dictionary<string, (DelegateInvocationProxy Invocator, PropertyInfo Property)>? _invocators = new Dictionary<string, (DelegateInvocationProxy, PropertyInfo)>();

        public ICollection<TObject> TrackedInstances { get; }

        private void InstancePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Debug.Assert(_invocators != null);
            if (!_invocators.TryGetValue(e.PropertyName, out var invocatorDefinition))
                return;

            invocatorDefinition.Invocator.Invoke(sender, invocatorDefinition.Property.GetValue(sender));
        }

        public void ObserveProperty<TProperty>(Expression<Func<TObject, TProperty>> property, Action<TObject, TProperty> changedHandler)
        {
            if (!(property.Body is MemberExpression me && me.Member is PropertyInfo propertyInfo))
                throw new ArgumentException("Invalid property shape", nameof(property));

            if (changedHandler == null)
                throw new ArgumentNullException(nameof(changedHandler));

            DelegateInvocationProxy proxy;
            var invocators = _invocators ?? throw new ObjectDisposedException(GetType().FullName);

            if (invocators.TryGetValue(propertyInfo.Name, out var invocatorInfo))
            {
                proxy = invocatorInfo.Invocator;
            }
            else
            {
                proxy = new DelegateInvocator<TObject, TProperty>();
                invocators.Add(propertyInfo.Name, (proxy, propertyInfo));
            }
            proxy.Add(changedHandler);
        }

        public void RemoveObserver<TProperty>(Expression<Func<TObject, TProperty>> property, Action<TObject, TProperty> changeHandler)
        {
            if (!(property.Body is MemberExpression me && me.Member is PropertyInfo propertyInfo))
                throw new ArgumentException("Invalid property shape", nameof(property));

            if (changeHandler == null)
                throw new ArgumentNullException(nameof(changeHandler));
            _invocators?[propertyInfo.Name].Invocator.Subtract(changeHandler);
        }

        public void RemoveObserver<TProperty>(string propertyName, Action<TObject, TProperty> changeHandler)
        {
            _invocators?[propertyName].Invocator.Subtract(changeHandler);
        }



        public void Dispose()
        {
            _trackedInstances?.Clear();
            if (_invocators != null)
            {
                _invocators.Clear();
                _invocators = null;
            }

        }
    }



}
