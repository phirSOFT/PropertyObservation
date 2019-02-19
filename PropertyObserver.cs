using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

#nullable enable
namespace phirSOFT.PropertyObservation
{
    public class PropertyObserver<TObject> where TObject : class, INotifyPropertyChanged, IDisposable
    {
        private TObject? _instance;

        public PropertyObserver(TObject instance)
        {
            _instance = instance;
        }

        private Dictionary<string, (DelegateInvocationProxy Invocator, PropertyInfo Property)>? _invocators = new Dictionary<string, (DelegateInvocationProxy, PropertyInfo)>();

        public TObject Instance
        {
            get => _instance ?? throw new ObjectDisposedException(GetType().FullName);
            set
            {
                if(ReferenceEquals(_instance, value))
                    return;
                if (_instance != null)
                {
                    _instance.PropertyChanged -= InstancePropertyChanged;
                }

                _instance = value;

                if (value != null)
                {
                    value.PropertyChanged += InstancePropertyChanged;
                }
            }
        }

        private void InstancePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Debug.Assert(_invocators != null);
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
            var invocators = _invocators?? throw new ObjectDisposedException(GetType().FullName);

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

        public void RemoveObserver<TProperty>(Expression<Func<TObject, TProperty>> property, Action<TObject, TProperty> changedHandler)
        {
            if (!(property.Body is MemberExpression me && me.Member is PropertyInfo propertyInfo))
                throw new ArgumentException("Invalid property shape", nameof(property));

            if (changedHandler == null)
                throw new ArgumentNullException(nameof(changedHandler));
            _invocators?[propertyInfo.Name].Invocator.Subtract(changedHandler);
        }

        public void Dispose()
        {
            if(_instance != null)
            {
                _instance.PropertyChanged -= InstancePropertyChanged;
                _instance = null;
            }
            if(_invocators != null)
            {
                _invocators.Clear();
                _invocators = null;
            }

        }
    }

}
