using System;
using System.Reflection;

namespace PiTung_Bootstrap
{
    internal struct MethodPatch
    {
        public MethodInfo BaseMethod { get; }
        public MethodInfo PatchMethod { get; }
        public bool Prefix { get; }
        public bool Postfix { get; }
        public Type[] ParameterTypes { get; }

        public MethodPatch(MethodInfo baseMethod, MethodInfo patchMethod, bool prefix, Type[] paramTypes = default(Type[]))
        {
            this.BaseMethod = baseMethod;
            this.PatchMethod = patchMethod;
            this.Prefix = prefix;
            this.Postfix = !prefix;
            this.ParameterTypes = paramTypes;
        }
    }
}
