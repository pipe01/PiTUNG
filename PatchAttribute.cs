using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PiTung_Bootstrap
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class PatchAttribute : Attribute
    {
        readonly Type containerType;
        readonly string methodName;
        
        internal PatchAttribute(Type containerType, string methodName)
        {
            this.containerType = containerType;
            this.methodName = methodName;
        }

        public Type ContainerType => containerType;
        public string MethodName => methodName;
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class PrefixAttribute : PatchAttribute
    {
        /// <summary>
        /// Marks this method as a prefix patch. Prefix patches get executed before the original method.
        /// This method may optionally return a boolean value indicating whether or not the original method
        /// should be executed.
        /// <para>
        /// If the prefix method contains a __instance parameter of the same type as <paramref name="containerType"/>,
        /// its value will be equivalent to the keyword 'this'.
        /// </para>
        /// </summary>
        /// <param name="containerType">The type that contains the method to be patched.</param>
        /// <param name="methodName">The name of the original method.</param>
        public PrefixAttribute(Type containerType, string methodName) : base(containerType, methodName)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class PostfixAttribute : PatchAttribute
    {
        /// <summary>
        /// Makrs this method as a postfix patch. Postfix patches get executed after the original method.
        /// <para/>
        /// If the postfix method contains a __instance parameter of the same type as <paramref name="containerType"/>,
        /// its value will be equivalent to the keyword 'this'.
        /// </summary>
        /// <param name="containerType">The type that contains the method to be patched.</param>
        /// <param name="methodName">The name of the original method.</param>
        public PostfixAttribute(Type containerType, string methodName) : base(containerType, methodName)
        {
        }
    }
}
