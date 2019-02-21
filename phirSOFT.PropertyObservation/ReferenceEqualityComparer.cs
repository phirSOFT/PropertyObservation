using System;
using System.Collections.Generic;
using System.Text;

namespace phirSOFT.PropertyObservation
{
    internal class ReferenceEqualityComparer<TObject> : IEqualityComparer<TObject> where TObject : class
    {
        private static readonly Lazy<ReferenceEqualityComparer<TObject>> _comparer = new Lazy<ReferenceEqualityComparer<TObject>>();
        public static IEqualityComparer<TObject> Comparer => _comparer.Value;

        private ReferenceEqualityComparer()
        {

        }

        public bool Equals(TObject x, TObject y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(TObject obj)
        {
           return obj.GetHashCode();
        }
    }
}
