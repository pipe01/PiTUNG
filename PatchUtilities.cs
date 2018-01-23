using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PiTung_Bootstrap
{
    internal static class PatchUtilities
    {
        public static IEnumerable<MethodPatch> GetMethodPatches(Type patchType, Type targetType)
        {
            if (patchType == null)
                throw new ArgumentNullException(nameof(patchType));
            
            var methods = patchType.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            
            foreach (var item in methods)
            {
                PatchMethodAttribute attribute = item.GetAttribute<PatchMethodAttribute>();

                MDebug.WriteLine(item.Name);

                if (attribute == null)
                    continue;
                
                bool prefix = attribute.PatchType == PatchType.Prefix,
                     postfix = attribute.PatchType == PatchType.Postfix;

                if (item.ReturnType != typeof(void) && postfix)
                {
                    throw new Exception("Postfix patch methods cannot return anything other than void.");
                }
                if (prefix && item.ReturnType != typeof(void) && item.ReturnType != typeof(bool))
                {
                    throw new Exception("Prefix patch methods can only return either void or bool");
                }

                string originalMethodName = attribute.OriginalMethod ?? item.Name;

                var originalMethod = targetType.GetMethod(originalMethodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

                if (originalMethod == null)
                {
                    throw new Exception($"Method '{originalMethodName}' not found in type '{targetType.Name}'");
                }

                yield return new MethodPatch(originalMethod, item, attribute.PatchType);
            }
        }
    }
}
