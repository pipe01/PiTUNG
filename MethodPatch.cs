using System;
using System.Reflection;

namespace PiTung_Bootstrap
{
    public enum PatchType
    {
        Prefix,
        Postfix,
        //NewMethod
    }

    internal struct MethodPatch
    {
        public MethodInfo BaseMethod { get; }
        public MethodInfo PatchMethod { get; }
        public PatchType Type { get; }
        public Type[] ParameterTypes { get; }

        public bool Prefix => Type == PatchType.Prefix;
        public bool Postfix => Type == PatchType.Postfix;

        public MethodPatch(MethodInfo baseMethod, MethodInfo patchMethod, PatchType type, Type[] paramTypes = default(Type[]))
        {
            this.BaseMethod = baseMethod;
            this.PatchMethod = patchMethod;
            this.Type = type;
            this.ParameterTypes = paramTypes;
        }
    }
}
