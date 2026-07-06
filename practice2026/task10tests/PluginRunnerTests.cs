using System;
using System.Collections.Generic;
using task07;
using task10;

namespace task10tests;

public class PluginRunnerTests
{
    [PluginLoad]
    private class PluginA { }
    [PluginLoad("PluginA")]
    private class PluginB { }
    [PluginLoad("PluginB")]
    private class PluginC { }

    [PluginLoad("PluginY")]
    private class PluginX { }
    [PluginLoad("PluginX")]
    private class PluginY { }

    [PluginLoad("NonExistentPlugin")]
    private class BadPlugin { }

    [Fact]
    public void SortPlugins_ShouldCorrectSequence_ForValidDependencies()
    {
        var plugins = new List<Type> { typeof(PluginB), typeof(PluginA), typeof(PluginC) };

        var graph_dependencies = PluginRunner.GetDependencyGraph(plugins);
        var sorted_plugins = PluginRunner.SortPlugins(graph_dependencies);

        Assert.Equal(3, sorted_plugins.Count);
        int indexA = sorted_plugins.IndexOf(typeof(PluginA));
        int indexB = sorted_plugins.IndexOf(typeof(PluginB));
        int indexC = sorted_plugins.IndexOf(typeof(PluginC));
        Assert.True(indexA < indexB);
        Assert.True(indexB < indexC);
    }

    [Fact]
    public void GetDependencyGraph_ShouldThrowException_ForNoExistentDependency()
    {
        var plugins = new List<Type> { typeof(BadPlugin) };

        try
        {
            PluginRunner.GetDependencyGraph(plugins);
            Assert.Fail("Метод должен был выбросить ошибку из-за отсутствия зависимости");
        }
        catch (Exception ex) { Assert.Contains("она не найдена", ex.Message); }
    }

    [Fact]
    public void SortPlugins_ShouldThrowException_ForСyclicDependency()
    {
        var plugins = new List<Type> { typeof(PluginY), typeof(PluginX) };
        var graph_dependencies = PluginRunner.GetDependencyGraph(plugins);

        try
        {
            PluginRunner.SortPlugins(graph_dependencies);
            Assert.Fail("Метод должен был выбросить ошибку из-за циклической зависимости");
        }
        catch (Exception ex) { Assert.Contains("Обнаружена циклическая зависимость!", ex.Message); }
    }
}