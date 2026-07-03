using System.Reflection;

class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Пустой путь");
            return;
        }

        string dllPath = args[0];

        if (!File.Exists(dllPath))
        {
            Console.WriteLine("Файл по заданному пути не найден");
            return;
        }

        var assembly = Assembly.LoadFrom(dllPath);
        foreach (var type in assembly.GetTypes())
        {
            Console.WriteLine($"\nКласс: {type.FullName}");
            foreach (var attribute in type.GetCustomAttributes())
            {
                Console.WriteLine($"Атрибут: {attribute}");
            }

            foreach (var constructor in type.GetConstructors())
            {
                Console.WriteLine($"Конструктор: {constructor}");
                if (constructor.GetParameters().Length != 0)
                {
                    Console.WriteLine($"\tПараметры:");
                    foreach (var parameter in constructor.GetParameters())
                    {
                        Console.WriteLine($"\t{parameter.Name} {parameter.ParameterType.Name}");
                    }
                }
            }

            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                Console.WriteLine($"Метод: {method.Name}");
                if (method.GetParameters().Length != 0)
                {
                    Console.WriteLine($"\tПараметры:");
                    foreach (var parameter in method.GetParameters())
                    {
                        Console.WriteLine($"\t{parameter.Name} {parameter.ParameterType.Name}");
                    }
                }
            }
        }
    }
}

