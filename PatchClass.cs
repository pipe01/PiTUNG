using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PiTung_Bootstrap
{
    /// <summary>
    /// Represents a set of patches dedicated to a single target class.
    /// </summary>
    /// <typeparam name="TClass">The class type that contains the methods we want to patch.</typeparam>
    public abstract class PatchClass<TClass>
    {
        internal IEnumerable<MethodPatch> GetMethodPatches()
        {
            Type t = this.GetType();

            var methods = t.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

            foreach (var item in methods)
            {
                bool prefix  = item.Name.EndsWith("Prefix"),
                     postfix = item.Name.EndsWith("Postfix");
                
                if (!prefix && !postfix)
                {
                    //TODO Maybe log that there is a method that isn't a prefix or a postfix.
                    continue;
                }

                if (item.ReturnType != typeof(void) && postfix)
                {
                    throw new Exception("Postfix patch methods cannot return anything other than void.");
                }
                if (prefix && item.ReturnType != typeof(void) && item.ReturnType != typeof(bool))
                {
                    throw new Exception("Prefix patch methods can only return either void or bool");
                }

                string originalMethodName = item.Name.Substring(0, item.Name.Length - (prefix ? "Prefix" : "Postfix").Length);

                var originalMethod = typeof(TClass).GetMethod(originalMethodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

                if (originalMethod == null)
                {
                    throw new Exception($"Method '{originalMethodName}' not found in type '{typeof(TClass).Name}'");
                }

                yield return new MethodPatch(originalMethod, item, prefix, postfix);
            }
        }
    }
}
