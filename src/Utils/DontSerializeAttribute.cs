using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PiTung.Utils
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    internal class DontSerializeAttribute : Attribute
    {
    }
}
