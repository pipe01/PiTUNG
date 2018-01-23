using System.Linq;
using System.Diagnostics;
using PiTung_Bootstrap;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Harmony;

namespace PiTung_Bootstrap_Tests
{
    [TestClass]
    public class PatchClassTest
    {
        //private const string OriginalString = "This is the original function!";

        //private class DummyClass
        //{
        //    public string MyVariable = "hello";

        //    public string MyFunction() => OriginalString;
        //}

        //private class TestPatchClass : PatchClass<DummyClass>
        //{
        //    static void MyFunctionPrefix(DummyClass __instance)
        //    {
        //        Debug.WriteLine("Patch function! " + __instance.MyVariable);
        //    }
        //}

        //private static HarmonyInstance _Harmony = HarmonyInstance.Create("test");

        //[TestMethod]
        //public void GetPatchMethodsTest()
        //{
        //    var patches = new TestPatchClass().GetMethodPatches().ToArray();

        //    Assert.AreEqual(1, patches.Length);
        //    Assert.AreEqual("MyFunction", patches[0].BaseMethod.Name);
        //    Assert.AreEqual("MyFunctionPrefix", patches[0].PatchMethod.Name);
        //    Assert.AreEqual(true, patches[0].Prefix);
        //    Assert.AreEqual(false, patches[0].Postfix);
        //}

        //[TestMethod]
        //public void PatchTest()
        //{
        //    var patches = new TestPatchClass().GetMethodPatches();

        //    foreach (var patch in patches)
        //    {
        //        if (patch.Prefix)
        //        {
        //            _Harmony.Patch(patch.BaseMethod, new HarmonyMethod(patch.PatchMethod), null);
        //        }
        //        else if (patch.Postfix)
        //        {
        //            _Harmony.Patch(patch.BaseMethod, null, new HarmonyMethod(patch.PatchMethod));
        //        }
        //    }

        //    new DummyClass().MyFunction();
        //}
    }
}
