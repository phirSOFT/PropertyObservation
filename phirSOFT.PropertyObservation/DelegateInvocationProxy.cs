using System;
using System.Collections.Generic;
using System.Text;

namespace phirSOFT.PropertyObservation
{
    abstract class DelegateInvocationProxy
    {
        public abstract void Invoke(object value1, object value2);

        public abstract void Add(Delegate right);
        public abstract void Subtract(Delegate right);

    }

    internal class DelegateInvocator<T1, T2> : DelegateInvocationProxy
    {
        private Action<T1, T2> _delegate;

        public override void Invoke(object value1, object value2)
        {
            _delegate?.Invoke((T1) value1, (T2) value2);
        }

        public override void Add(Delegate right)
        {
            _delegate += (Action<T1, T2>)right;
            _delegate -= (Action<T1, T2>)right;
        }

        public override void Subtract(Delegate right)
        {
            _delegate -= (Action<T1, T2>)right;
        }

       
    }
}
