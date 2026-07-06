using System.Reflection;
using task07;

namespace task10;

public class PluginRunner
{
    static void Main()
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        try
        {
            List<Type> classesTypes = SearchDll(baseDirectory);
            Dictionary<Type, List<Type>> dependencyGraph = GetDependencyGraph(classesTypes);
            List<Type> sortedPlugins = SortPlugins(dependencyGraph);
            RunPlugins(sortedPlugins);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[Ошибка системы]: {ex.Message}");
            Console.ResetColor();
        }
    }

    static List<Type> SearchDll(string directoryPath)
    {
        var classesTypes = new List<Type>();

        if (!Directory.Exists(directoryPath)) return classesTypes;

        string[] dllFiles = Directory.GetFiles(directoryPath, "*.dll");
        foreach (var dll in dllFiles)
        {
            try
            {
                Assembly assembly = Assembly.LoadFrom(dll);
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsClass && !type.IsAbstract && Attribute.IsDefined(type, typeof(PluginLoadAttribute)))
                    {
                        classesTypes.Add(type);
                    }
                }
            }
            catch { }
        }
        return classesTypes;
    }

    public static Dictionary<Type, List<Type>> GetDependencyGraph(List<Type> classesTypes)
    {
        var dependencyGraph = new Dictionary<Type, List<Type>>();

        foreach (var classType in classesTypes)
        {
            dependencyGraph[classType] = new List<Type>();

            var attribute = classType.GetCustomAttribute<PluginLoadAttribute>();
            if (attribute?.Dependencies != null)
            {
                foreach (var depName in attribute.Dependencies)
                {
                    var dependencyType = classesTypes.Find(p => p.Name == depName);
                    if (dependencyType != null)
                    {
                        dependencyGraph[classType].Add(dependencyType);
                    }
                    else
                    {
                        throw new Exception($"Плагин {classType.Name} требует зависимость '{depName}', но она не найдена в папке.");
                    }
                }
            }
        }
        return dependencyGraph;
    }

    public static List<Type> SortPlugins(Dictionary<Type, List<Type>> dependencyGraph)
    {
        var sortedPlugins = new List<Type>();
        var visited = new Dictionary<Type, bool>();

        foreach (var plugin in dependencyGraph.Keys)
        {
            Visit(plugin, dependencyGraph, visited, sortedPlugins);
        }
        return sortedPlugins;
    }

    static void Visit(Type plugin, Dictionary<Type, List<Type>> dependencyGraph, Dictionary<Type, bool> visited, List<Type> sortedPlugins)
    {
        if (visited.TryGetValue(plugin, out bool isFullyVisited))
        {
            if (!isFullyVisited)
                throw new Exception($"Обнаружена циклическая зависимость! Компонент '{plugin.Name}' зациклен сам на себя.");
            return;
        }

        visited[plugin] = false;

        foreach (var dependency in dependencyGraph[plugin])
        {
            Visit(dependency, dependencyGraph, visited, sortedPlugins);
        }

        visited[plugin] = true;
        sortedPlugins.Add(plugin);
    }

    static void RunPlugins(List<Type> sortedPlugins)
    {
        Console.WriteLine("\n--- Запуск модулей системы ---");
        foreach (var type in sortedPlugins)
        {
            Console.WriteLine($"[Система]: Запуск {type.Name}...");

            var instance = Activator.CreateInstance(type);
            var method = type.GetMethod("Execute", BindingFlags.Public | BindingFlags.Instance);

            if (method != null)
            {
                method.Invoke(instance, null);
            }
            else
            {
                throw new Exception($"У плагина {type.Name} отсутствует обязательный метод 'Execute()'.");
            }
        }
    }
}