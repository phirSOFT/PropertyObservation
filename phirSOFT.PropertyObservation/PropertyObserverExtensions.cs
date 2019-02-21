using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Reflection;

namespace phirSOFT.PropertyObservation
{
    public static class PropertyObserverExtensions
    {
        public static PropertyObserver<TObject> ObserveProperty<TObject, TProperty>(this TObject targetObject, Expression<Func<TObject, TProperty>> property, Action<TObject, TProperty> onChanged)
            where TObject : class, INotifyPropertyChanged
        {
            var observer = new PropertyObserver<TObject>() { TrackedInstances = {targetObject}};
            observer.ObserveProperty(property, onChanged);
            return observer;
        }

#if netstandard13
        private static readonly ConditionalWeakTable<object, object> _syncProperties = new ConditionalWeakTable<object, object>();
        public static IDisposable SyncProperty<TLeft, TRight, TProperty>(
            this TLeft left,
            Expression<Func<TLeft, TProperty>> leftProperty,
            TRight right,
            Expression<Func<TRight, TProperty>> rightProperty,
            SyncDirection syncDirection = SyncDirection.Both
        )
            where TLeft : class, INotifyPropertyChanged
            where TRight : class, INotifyPropertyChanged
        {
            if ((syncDirection & SyncDirection.Both) == 0)
                throw new ArgumentOutOfRangeException(nameof(syncDirection));

            var syncHandle = new PropertySyncHandle<TLeft, TRight, TProperty>(left, leftProperty, right, rightProperty);

            if (syncDirection.HasFlag(SyncDirection.LeftToRight))
            {
                left.GetPropertyObserver().ObserveProperty(leftProperty, syncHandle.SyncLeft);
            }

            if (syncDirection.HasFlag(SyncDirection.RightToLeft))
            {
                right.GetPropertyObserver().ObserveProperty(rightProperty, syncHandle.SyncRight);
            }

            return syncHandle;
        }

        private static PropertyObserver<T> GetPropertyObserver<T>(this T instance) where T : class, INotifyPropertyChanged
        {
            return (PropertyObserver<T>)_syncProperties.GetValue(instance, instance => new PropertyObserver<T> { TrackedInstances = { (T)instance } });
        }

        private class PropertySyncHandle<TLeft, TRight, TProperty> : IDisposable
            where TLeft : class, INotifyPropertyChanged
            where TRight : class, INotifyPropertyChanged
        {
            private readonly PropertyInfo _leftProperty;
            private readonly PropertyInfo _rightProperty;
            private readonly WeakReference<TLeft> _left;
            private readonly WeakReference<TRight> _right;

            public PropertySyncHandle(
                 TLeft left,
                 Expression<Func<TLeft, TProperty>> leftProperty,
                 TRight right,
                 Expression<Func<TRight, TProperty>> rightProperty
                )
            {
                if(!(leftProperty is MemberExpression lmexpression && lmexpression.Member is PropertyInfo leftPropertyInfo))
                    throw new ArgumentException($"{leftProperty} could not be converteted to a property of {typeof(TLeft)}.", nameof(leftProperty));

                if (!(rightProperty is MemberExpression rmexpression && rmexpression.Member is PropertyInfo rightPropertyInfo))
                    throw new ArgumentException($"{rightProperty} could not be converteted to a property of {typeof(TLeft)}.", nameof(leftProperty));

                _leftProperty = leftPropertyInfo;
                _rightProperty = rightPropertyInfo;
                _left = new WeakReference<TLeft>(left);
                _right = new WeakReference<TRight>(right);
            }

            public void Dispose()
            {
                if(_left.TryGetTarget(out var left))
                {
                    left.GetPropertyObserver().RemoveObserver<TProperty>(_leftProperty.Name, SyncLeft);
                }

                if (_right.TryGetTarget(out var right))
                {
                    right.GetPropertyObserver().RemoveObserver<TProperty>(_rightProperty.Name, SyncRight);
                }
            }

            internal void SyncLeft(TLeft sender, TProperty value)
            {
                if (_right.TryGetTarget(out var right))
                    _rightProperty.SetValue(right, value);
            }

            internal void SyncRight(TRight sender, TProperty value)
            {
                if (_left.TryGetTarget(out var left))
                    _rightProperty.SetValue(left, value);
            }
        }
#endif
    }
}
