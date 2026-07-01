using System;
using System.Reflection;
using System.Collections.Generic;

namespace task05
{
    public class ClassAnalyzer
    {
        private Type _type;

        public ClassAnalyzer(Type type)
        {
            _type = type;
        }

        public IEnumerable<string> GetPublicMethods()
        {
            var query = from method in _type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                        where !method.IsSpecialName
                        select method.Name;
            return query;
        }
        public IEnumerable<string> GetMethodParams(string methodname)
        {
            var method = _type.GetMethod(methodname, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            if (method == null) return Enumerable.Empty<string>();

            var return_type = new[] { method.ReturnType.Name };
            var param_query = from param in method.GetParameters()
                              select $"{param.ParameterType.Name} {param.Name}";

            return return_type.Concat(param_query);
        }
        public IEnumerable<string> GetAllFields()
        {
            var query = from field in _type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                        where !field.Name.EndsWith("__BackingField")
                        select field.Name;
            return query;
        }
        public IEnumerable<string> GetProperties()
        {
            var query = from prop in _type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                        select prop.Name;
            return query;
        }
        public bool HasAttribute<T>() where T : Attribute
        {
            return _type.GetCustomAttribute<T>() != null;
        }
    }
}