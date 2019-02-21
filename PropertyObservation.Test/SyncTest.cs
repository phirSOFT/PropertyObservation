using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using phirSOFT.PropertyObservation;
namespace PropertyObservation.Test
{
    public class SyncTest
    {
        [Test]
        public void SyncBidireectional()
        {
            var a = new Mocks.ObservableMock<int>();
            var b = new Mocks.ObservableMock<int>();

            using (a.SyncProperty(x => x.Property, b, x => x.OtherProperty, SyncDirection.Both))
            {
                a.Property = 4;
                Assert.AreEqual(4, b.OtherProperty);
                b.OtherProperty = 7;
                Assert.AreEqual(7, a.Property);
            }

            a.Property = 9;
            Assert.AreEqual(7, b.OtherProperty);
        }

    }
}
