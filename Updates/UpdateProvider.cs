using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PiTung_Bootstrap.Updates
{
    public abstract class UpdateProvider
    {
        private Update LastUpdate;

        protected string ModPackage { get; }
        protected Mod LocalMod => Bootstrapper.Mods.SingleOrDefault(o => o.PackageName.Equals(ModPackage, StringComparison.InvariantCulture));

        internal abstract Update GetUpdate();

        public UpdateProvider(string modPackage)
        {
            this.ModPackage = modPackage;
        }

        public bool IsUpdateAvailable()
        {
            if (LastUpdate == null)
                LastUpdate = GetUpdate();

            return LastUpdate.NewVersion > LocalMod.ModVersion;
        }
    }
}
