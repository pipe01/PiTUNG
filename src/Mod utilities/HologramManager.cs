using System.Collections.Generic;

namespace PiTung.Mod_utilities
{
    internal static class HologramManager
    {
        public static IList<Hologram> ActiveHolograms = new List<Hologram>();

        public static void Draw()
        {
            foreach (var item in ActiveHolograms)
            {
                item.Draw();
            }
        }

        public static void Update()
        {
            foreach (var item in ActiveHolograms)
            {
                item.Update();
            }
        }
    }
}
