using System.Reflection;

namespace task07
{
    public class DisplayNameAttribute : Attribute
    {
        public string DisplayName { get; }
        public DisplayNameAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }

    public class VersionAttribute : Attribute
    {
        public int Major { get; }
        public int Minor { get; }
        public VersionAttribute(int major, int minor)
        {
            Major = major;
            Minor = minor;
        }
    }

    [DisplayName("Пример класса")]
    [Version(1, 0)]
    public class SampleClass
    {
        [DisplayName("Числовое свойство")]
        public int Number { get; set; }

        [DisplayName("Тестовый метод")]
        public void TestMethod() { }
    }

    public static class ReflectionHelper
    {
        public static void PrintTypeInfo(Type type)
        {
            var classDisplay = type.GetCustomAttribute<DisplayNameAttribute>();
            if (classDisplay != null)
            {
                Console.WriteLine($"Отображаемое имя класса: {classDisplay.DisplayName}");
            }

            var classVersion = type.GetCustomAttribute<VersionAttribute>();
            if (classVersion != null)
            {
                Console.WriteLine($"Версия класса: {classVersion.Major}.{classVersion.Minor}");
            }

            Console.WriteLine("Свойства:");
            foreach (var prop in type.GetProperties())
            {
                var propDisplay = prop.GetCustomAttribute<DisplayNameAttribute>();
                if (propDisplay != null)
                {
                    Console.WriteLine($"- {prop.Name}: {propDisplay.DisplayName}");
                }
            }

            Console.WriteLine("Методы:");
            foreach (var method in type.GetMethods())
            {
                var methodDisplay = method.GetCustomAttribute<task07.DisplayNameAttribute>();
                if (methodDisplay != null)
                {
                    Console.WriteLine($"- {method.Name}: {methodDisplay.DisplayName}");
                }
            }
        }
    }
}
