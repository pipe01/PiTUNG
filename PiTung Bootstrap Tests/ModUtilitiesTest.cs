using PiTung_Bootstrap;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PiTung_Bootstrap_Tests
{
    [TestClass]
    public class ModUtilitiesTest
    {
        private class TestClass
        {
            public string PublicField = "hello";
            private int PrivateField = 42;

            public int GetPrivateField() => PrivateField;
        }

        [TestMethod]
        public void SetPublicField()
        {
            var cls = new TestClass();

            ModUtilities.SetFieldValue(cls, "PublicField", "goodbye", false);

            Assert.AreEqual("goodbye", cls.PublicField);
        }

        [TestMethod]
        public void SetPrivateField()
        {
            var cls = new TestClass();

            ModUtilities.SetFieldValue(cls, "PrivateField", 21, true);

            Assert.AreEqual(21, cls.GetPrivateField());
        }

        [TestMethod]
        public void GetPublicField()
        {
            var cls = new TestClass();

            Assert.AreEqual(cls.PublicField, ModUtilities.GetFieldValue<string>(cls, "PublicField", false));
        }

        [TestMethod]
        public void GetPrivateField()
        {
            var cls = new TestClass();

            Assert.AreEqual(cls.GetPrivateField(), ModUtilities.GetFieldValue<int>(cls, "PrivateField", true));
        }
    }
}
