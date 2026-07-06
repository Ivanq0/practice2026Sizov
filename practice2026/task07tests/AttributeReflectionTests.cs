using System.ComponentModel;
using System.Reflection;
using task07;

public class AttributeReflectionTests
{
    [Fact]
    public void SampleClass_HasDisplayNameAttribute()
    {
        var type = typeof(SampleClass);
        var attribute = type.GetCustomAttribute<task07.DisplayNameAttribute>();
        Assert.NotNull(attribute);
        Assert.Equal("Пример класса", attribute.DisplayName);
    }

    [Fact]
    public void SampleClassMethod_HasDisplayNameAttribute()
    {
        var method = typeof(SampleClass).GetMethod("TestMethod");
        var attribute = method.GetCustomAttribute<task07.DisplayNameAttribute>();
        Assert.NotNull(attribute);
        Assert.Equal("Тестовый метод", attribute.DisplayName);
    }

    [Fact]
    public void SampleClassProperty_HasDisplayNameAttribute()
    {
        var prop = typeof(SampleClass).GetProperty("Number");
        var attribute = prop.GetCustomAttribute<task07.DisplayNameAttribute>();
        Assert.NotNull(attribute);
        Assert.Equal("Числовое свойство", attribute.DisplayName);
    }

    [Fact]
    public void SampleClass_HasVersionAttribute()
    {
        var type = typeof(SampleClass);
        var attribute = type.GetCustomAttribute<VersionAttribute>();
        Assert.NotNull(attribute);
        Assert.Equal(1, attribute.Major);
        Assert.Equal(0, attribute.Minor);
    }

    [Fact]
    public void ReflectionHelper_ForSampleClass_ShouldGenerateCorrectResult()
    {
        string result = ReflectionHelper.PrintTypeInfo(typeof(SampleClass));

        Assert.Contains("Отображаемое имя класса: Пример класса", result);
        Assert.Contains("Версия класса: 1.0", result);
        Assert.Contains("- TestMethod: Тестовый метод", result);
        Assert.Contains("- Number: Числовое свойство", result);
    }
}