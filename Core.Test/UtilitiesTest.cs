using System;
using NUnit.Framework;

namespace Core.Test
{
    [TestFixture]
    public class UtilitiesTest
    {
        [Test]
        public void ThrowIfNull_does_throw_when_parameter_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => Utilities.ThrowIfNull(null, "null"));
        }

        [Test]
        public void ThrowIfNull_does_not_throw_when_parameter_is_not_null()
        {
            Assert.DoesNotThrow(() => Utilities.ThrowIfNull(new object(), "valid object"));
        }
    }
}