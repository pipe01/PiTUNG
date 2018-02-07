using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PiTung_Bootstrap.Updates
{
    public class Update
    {
        public string ModPackage { get; }
        public Version NewVersion { get; }
        public string DownloadUrl { get; }

        public Update(string modPackage, Version newVer, string downloadUrl)
        {
            this.ModPackage = modPackage;
            this.NewVersion = newVer;
            this.DownloadUrl = downloadUrl;
        }
    }
}
