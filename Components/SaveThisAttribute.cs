using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PiTung.Components
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    public sealed class SaveThisAttribute : Attribute
    {
    }
}
