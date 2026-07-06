using System.Reflection;
using System.Text;

namespace task07
{
    public static class ReflectionHelper
    {
        public static string PrintTypeInfo(Type type)
        {
            var classDisplay = type.GetCustomAttribute<DisplayNameAttribute>();
            var sb = new StringBuilder();
            if (classDisplay != null)
            {
                sb.AppendLine($"Отображаемое имя класса: {classDisplay.DisplayName}");
            }

            var classVersion = type.GetCustomAttribute<VersionAttribute>();
            if (classVersion != null)
            {
                sb.AppendLine($"Версия класса: {classVersion.Major}.{classVersion.Minor}");
            }

            sb.AppendLine("Свойства:");
            foreach (var prop in type.GetProperties())
            {
                var propDisplay = prop.GetCustomAttribute<DisplayNameAttribute>();
                if (propDisplay != null)
                {
                    sb.AppendLine($"- {prop.Name}: {propDisplay.DisplayName}");
                }
            }

            sb.AppendLine("Методы:");
            foreach (var method in type.GetMethods())
            {
                var methodDisplay = method.GetCustomAttribute<task07.DisplayNameAttribute>();
                if (methodDisplay != null)
                {
                    sb.AppendLine($"- {method.Name}: {methodDisplay.DisplayName}");
                }
            }
            return sb.ToString();
        }
    }
}
