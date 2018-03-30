using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PiTung.Components
{
    /// <summary>
    /// Marks this field as component data, which means that it will be saved and loaded along with the world.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public sealed class SaveThisAttribute : Attribute
    {
    }
}
