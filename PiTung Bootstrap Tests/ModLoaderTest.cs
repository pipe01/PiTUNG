using System.Linq;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PiTung_Bootstrap;

namespace PiTung_Bootstrap_Tests
{
    [TestClass]
    public class ModLoaderTest
    {
        [TestMethod]
        public void CheckIfAllModsAreLoaded()
        {
            var mods = ModLoader.GetMods();

            Assert.AreEqual(1, mods.Count());

            if (mods.Any())
            {
                var mod = mods.First();

                Assert.IsNotNull(mod.ModAssembly);
                Assert.IsNotNull(mod.Name);
                Assert.IsNotNull(mod.Author);
                Assert.IsNotNull(mod.ModVersion);
            }
        }
    }
}
