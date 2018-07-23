using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PiTung
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RequireModAttribute : Attribute
    {
        internal string ModPackage { get; }

        public RequireModAttribute(string modPackage)
        {
            this.ModPackage = modPackage;
        }
    }
}
