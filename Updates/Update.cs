using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PiTung_Bootstrap.Updates
{
    internal struct Update
    {
        public Mod Mod { get; }
        public Version NewVersion { get; }
        public string DownloadUrl { get; }

        public Update(Mod mod, Version newVer, string downloadUrl)
        {
            this.Mod = mod;
            this.NewVersion = newVer;
            this.DownloadUrl = downloadUrl;
        }
    }
}
