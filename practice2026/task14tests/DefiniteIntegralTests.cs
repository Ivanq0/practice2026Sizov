using System;
using Xunit;

public class DefiniteIntegralTests
{
    private readonly Func<double, double> X = x => x;
    private readonly Func<double, double> SIN = x => Math.Sin(x);

    [Fact]
    public void Test_Linear_Zero()
    {
        double res = DefiniteIntegral.Solve(-1, 1, X, 1e-4, 2);
        Assert.Equal(0, res, 1e-4);
    }

    [Fact]
    public void Test_Sin_Zero()
    {
        double res = DefiniteIntegral.Solve(-1, 1, SIN, 1e-5, 8);
        Assert.Equal(0, res, 1e-4);
    }

    [Fact]
    public void Test_Linear_Positive()
    {
        double res = DefiniteIntegral.Solve(0, 5, X, 1e-6, 8);

        Assert.Equal(12.5, res, 1e-5);
    }
}