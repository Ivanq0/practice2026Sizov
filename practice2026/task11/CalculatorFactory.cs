using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace task11;

public static class CalculatorFactory
{
    public static ICalculator CreateCalculator()
    {
        string originalCode = @"
        public class Calculator {
            public int Add(int a, int b) => a + b;
            public int Minus(int a, int b) => a - b;
            public int Mul(int a, int b) => a * b;
            public int Div(int a, int b) => a / b;
        }";

        string modifiedCode = originalCode.Replace("public class Calculator", "public class Calculator : task11.ICalculator");

        var syntaxTree = CSharpSyntaxTree.ParseText(modifiedCode);

        var references = new MetadataReference[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ICalculator).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Runtime")).Location)
        };

        var compilation = CSharpCompilation.Create(
            "DynamicCalculatorAssembly_" + Guid.NewGuid().ToString("N"),
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);

        if (!result.Success)
        {
            var failures = string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Select(d => d.GetMessage()));
            throw new InvalidOperationException($"Ошибка компиляции кода:\n{failures}");
        }

        ms.Seek(0, SeekOrigin.Begin);
        var assembly = Assembly.Load(ms.ToArray());

        var type = assembly.GetType("Calculator");

        return (ICalculator)Activator.CreateInstance(type);
    }
}