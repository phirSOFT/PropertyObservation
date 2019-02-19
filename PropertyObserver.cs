using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

#nullable enable
namespace PropertyObserving
{
    public class PropertyObserver<TObject> where TObject : class, INotifyPropertyChanged, IDisposable
    {
        private TObject? _instance;

        private Dictionary<string, (DelegateInvocationProxy Invocator, PropertyInfo Property)> _invocators = new Dictionary<string, (DelegateInvocationProxy, PropertyInfo)>();
        public TObject? Instance
        {
            get => _instance;
            set
            {
                if(ReferenceEquals(_instance, value))
                    return;

                if(value != null)
                {
                    value.PropertyChanged += InstancePropertyChanged;
                }
            };
        }

        private void InstancePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(!_invocators.TryGetValue(e.PropertyName, out var invocatorDefinition))
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
            if (_invocators.TryGetValue(propertyInfo.Name, out var invocatorInfo))
            {
               proxy = invocatorInfo.Invocator;
            }
            else
            {
                proxy = new DelegateInvocator<TObject, TProperty>();
                _invocators.Add(propertyInfo.Name, (proxy, propertyInfo));
            }
            proxy.Add(changedHandler)
        }

        public void RemoveObserver<TProperty>(Expression<Func<TObject, TProperty>> property, Action<TObject, TProperty> changedHandler)
        {
            if (!(property.Body is MemberExpression me && me.Member is PropertyInfo propertyInfo))
                throw new ArgumentException("Invalid property shape", nameof(property));

            if (changedHandler == null)
                throw new ArgumentNullException(nameof(changedHandler));

            DelegateInvocationProxy proxy;
            if (_invocators.TryGetValue(propertyInfo.Name, out var invocatorInfo))
            {
                proxy = invocatorInfo.Invocator;
            }
            _invocators[propertyInfo.Name].Invocator.Subtract(changedHandler);
        }
    }

}
