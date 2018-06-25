using System;
using NUnit.Framework;

namespace DataReceiver.Test
{
    [TestFixture]
    public class TestAThing
    {
        [Test]
        public void ThingDoesAThing()
        {
            Assert.That("A", Is.SameAs("A"));
        }
        [Test]
        public void ThingDdoesAThing()
        {
            Assert.That("b", Is.SameAs("A"));
        }
    }
}
