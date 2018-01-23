using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PiTung_Bootstrap
{
    internal static class Extensions
    {
        public static T GetAttribute<T>(this MemberInfo member) where T : Attribute
        {
            var attrs = member.GetCustomAttributes(typeof(T), false);

            return attrs.Length == 0 ? null : (T)attrs[0];
        }
    }
}
