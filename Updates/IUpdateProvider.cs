using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PiTung_Bootstrap.Updates
{
    internal interface IUpdateProvider
    {
        bool IsUpdateAvailable(Mod mod);
        Update GetUpdate(Mod mod);
    }
}
