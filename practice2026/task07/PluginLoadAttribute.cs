namespace task07;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class PluginLoadAttribute : Attribute
{
    public string[] Dependencies { get; }
    public PluginLoadAttribute(params string[] dependencies)
    {
        Dependencies = dependencies == null ? Array.Empty<string>() : dependencies;
    }
}