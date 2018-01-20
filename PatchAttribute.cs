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
        public PrefixAttribute(Type containerType, string methodName) : base(containerType, methodName)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class PostfixAttribute : PatchAttribute
    {
        public PostfixAttribute(Type containerType, string methodName) : base(containerType, methodName)
        {
        }
    }
}
