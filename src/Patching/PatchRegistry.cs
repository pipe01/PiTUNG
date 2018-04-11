using Harmony;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PiTung.Patching
{
    internal struct Patch
    {
        public Mod Mod { get; }
        public MethodInfo OriginalMethod { get; }
        public MethodInfo PatchMethod { get; }

        public Patch(Mod mod, MethodInfo originalMethod, MethodInfo patchMethod)
        {
            this.Mod = mod;
            this.OriginalMethod = originalMethod;
            this.PatchMethod = patchMethod;
        }
    }

    internal static class PatchRegistry
    {
        public static HarmonyInstance Harmony;
        public static IList<Patch> Patches = new List<Patch>();

        public static void UndoPatchesForMod(Mod mod)
        {
            foreach (var item in Patches.Where(o => o.Mod == mod).ToArray())
            {
                Harmony.RemovePatch(item.OriginalMethod, item.PatchMethod);

                Patches.Remove(item);
            }
        }
    }
}
