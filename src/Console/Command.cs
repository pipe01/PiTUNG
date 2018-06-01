using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PiTung.Console
{
    /// <summary>
    /// Represents a command that can be invoked from the console
    /// </summary>
    public abstract class Command
    {
        /// <summary>
        /// Used to invoke the command (e.g. "help")
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// How to use the command (e.g. $"{Name} argument [optional_argument]")
        /// </summary>
        public abstract string Usage { get; }

        /// <summary>
        /// Short description of what the command does, preferably on 1 line
        /// </summary>
        public virtual string Description { get; } = null;

        /// <summary>
        /// If false, this command won't be shown when executing "help" on the console.
        /// </summary>
        internal virtual bool ShowOnHelp { get; } = true;

        /// <summary>
        /// The mod that registered this command.
        /// </summary>
        internal Mod Mod { get; set; }

        /// <summary>
        /// Called when the command is invoked
        /// </summary>
        /// <param name="arguments">The arguments given to the command</param>
        /// <returns>False if the command was malformed (i.e., the command requires 2 arguments but only 1 was supplied).</returns>
        public abstract bool Execute(IEnumerable<string> arguments);

        /// <summary>
        /// Called when the auto-completion is triggered
        /// </summary>
        /// <param name="arguments">The arguments to the command, the last one needing auto-completion</param>
        /// <returns>The auto-completion candidates for the last argument</returns>
        public virtual IEnumerable<string> AutocompletionCandidates(IEnumerable<string> arguments) => Enumerable.Empty<string>();
    }
}
