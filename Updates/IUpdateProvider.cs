using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PiTung_Bootstrap.Updates
{
    public interface IUpdateProvider
    {
        bool IsUpdateAvailable();
        Update GetUpdate();
    }
}
