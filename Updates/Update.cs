using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PiTung_Bootstrap.Updates
{
    public struct Update
    {
        public string ModName { get; }
        public Version NewVersion { get; }
        public string DownloadUrl { get; }

        public Update(string modName, Version newVer, string downloadUrl)
        {
            this.ModName = modName;
            this.NewVersion = newVer;
            this.DownloadUrl = downloadUrl;
        }
    }
}
