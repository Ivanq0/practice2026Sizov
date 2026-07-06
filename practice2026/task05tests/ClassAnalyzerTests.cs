using Moq;
using System.Runtime.Serialization;
using task05;
using Xunit;

public class TestClass
{
    public int PublicField;
    private string _privateField;
    public int Property { get; set; }

    public void Method() { }

    public int MethodWithParams(int param_one, string param_two)
    {
        return 1;
    }

    private void PrivateMethod() { }
}

[Serializable]
public class AttributedClass { }

public class ClassAnalyzerTests
{
    [Fact]
    public void GetPublicMethods_ReturnsCorrectMethods()
    {
        var analyzer = new ClassAnalyzer(typeof(TestClass));
        var methods = analyzer.GetPublicMethods();

        Assert.Contains("Method", methods);
        Assert.Contains("MethodWithParams", methods);
        Assert.Equal(2, methods.Count());
    }

    [Fact]
    public void GetMethodParams_ReturnsReturnTypeAndParameters_WhenMethodExists()
    {
        var analyzer = new ClassAnalyzer(typeof(TestClass));
        var res = analyzer.GetMethodParams("MethodWithParams").ToList();

        Assert.Equal(3, res.Count);
        Assert.Equal("Int32", res[0]);
        Assert.Equal("Int32 param_one", res[1]);
        Assert.Equal("String param_two", res[2]);
    }

    [Fact]
    public void GetMethodParams_ReturnsReturnTypeAndParameters_WhenMethodDoesNotExists()
    {
        var analyzer = new ClassAnalyzer(typeof(TestClass));
        var res = analyzer.GetMethodParams("ImaginaryMethod").ToList();

        Assert.Empty(res);
    }

    [Fact]
    public void GetMethodParams_ReturnsReturnTypeAndParameters_WhenMethodDoesNotHaveParams()
    {
        var analyzer = new ClassAnalyzer(typeof(TestClass));
        var res = analyzer.GetMethodParams("Method").ToList();

        Assert.Single(res);
        Assert.Equal("Void", res[0]);
    }

    [Fact]
    public void GetAllFields_ReturnsCorrectFields()
    {
        var analyzer = new ClassAnalyzer(typeof(TestClass));
        var fields = analyzer.GetAllFields();

        Assert.Contains("PublicField", fields);
        Assert.Contains("_privateField", fields);
        Assert.Equal(2, fields.Count());
    }

    [Fact]
    public void GetProperties_ReturnsCorrectProperties()
    {
        var analyzer = new ClassAnalyzer(typeof(TestClass));
        var properties = analyzer.GetProperties();

        Assert.Contains("Property", properties);
        Assert.Single(properties);
    }
    [Fact]
    public void HasAttribute_ReturnsTrue()
    {
        var analyzer = new ClassAnalyzer(typeof(AttributedClass));
        var res = analyzer.HasAttribute<SerializableAttribute>();

        Assert.True(res);
    }
    [Fact]
    public void HasAttribute_ReturnsFalse()
    {
        var analyzer = new ClassAnalyzer(typeof(TestClass));
        var res = analyzer.HasAttribute<SerializableAttribute>();

        Assert.False(res);
    }
}