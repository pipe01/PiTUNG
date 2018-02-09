using System;

namespace PiTung_Bootstrap
{
    /// <summary>
    /// If <see cref="OriginalMethod"/> is not specified, it will be assumed that the original method's name
    /// is the same as this method's name. If <see cref="PatchType"/>, it will be assumed that the method
    /// is a prefix patch.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class PatchMethodAttribute : Attribute
    {
        public string OriginalMethod { get; set; } = null;
        public PatchType PatchType { get; set; } = PatchType.Prefix;
    }
}
