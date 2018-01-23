using System;

namespace PiTung_Bootstrap
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class TargetAttribute : Attribute
    {
        /// <summary>
        /// Marks this class as a patch class. 
        /// <para />
        /// Patch classes may contains static methods in order to patch existing
        /// ones, named in the following format: OriginalMethodName[Prefix|Postfix]. If no patch type 
        /// (prefix or postfix) is specified, prefix will selected by default.
        /// </summary>
        /// <param name="containerType">The type that contains the method we want to patch.</param>
        public TargetAttribute(Type containerType)
        {
            this.ContainerType = containerType;
        }

        public Type ContainerType { get; }
    }
}
