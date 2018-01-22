using System.Reflection;

namespace PiTung_Bootstrap
{
    internal struct MethodPatch
    {
        public MethodInfo BaseMethod { get; }
        public MethodInfo PatchMethod { get; }
        public bool Prefix { get; }
        public bool Postfix { get; }

        public MethodPatch(MethodInfo baseMethod, MethodInfo patchMethod, bool prefix, bool postfix)
        {
            this.BaseMethod = baseMethod;
            this.PatchMethod = patchMethod;
            this.Prefix = prefix;
            this.Postfix = postfix;
        }
    }
}
