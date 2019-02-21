using NUnit.Framework;
using phirSOFT.PropertyObservation;

namespace PropertyObservation.Test
{
    public class PropertyObserverTest
    {

        [Test]
        public void BasicObservation()
        {
            bool fired = false;
            var obj = new Mocks.ObservableMock<int>();
            using (obj.ObserveProperty(o => o.Property, (sende, v) => fired = true))
            {
                Assert.False(fired);
                obj.Property = 1;
            }
            Assert.True(fired);
        }

        [Test]
        public void CrossObservation()
        {
            bool propertyFired = false;
            bool otherPropertyFired = false;
            var obj = new Mocks.ObservableMock<int>();
            using (obj.ObserveProperty(o => o.Property, (sende, v) => propertyFired = true))
            {
                obj.ObserveProperty(o => o.OtherProperty, (sender, v) => otherPropertyFired = true);
                Assert.False(propertyFired);
                Assert.False(otherPropertyFired);
                obj.Property = 1;
                Assert.True(propertyFired);
                Assert.False(otherPropertyFired);
            }
        }
    }
}