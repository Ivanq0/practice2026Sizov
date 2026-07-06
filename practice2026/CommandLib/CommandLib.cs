using System;

namespace CommandLib
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class PluginLoadAttribute : Attribute
    {
        public string[] Dependencies { get; }

        public PluginLoadAttribute(params string[] dependencies)
        {
            Dependencies = dependencies ?? Array.Empty<string>();
        }
    }
}