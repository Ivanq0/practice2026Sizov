using System;
using Xunit;
using task11;

namespace task11tests;

public class CalculatorTests
{
    [Fact]
    public void CreateCalculator_ShouldPerformAllArithmeticOperationsCorrectly()
    {
        ICalculator calculator = CalculatorFactory.CreateCalculator();

        Assert.NotNull(calculator);
        Assert.Equal(8, calculator.Add(5, 3));
        Assert.Equal(2, calculator.Minus(5, 3));
        Assert.Equal(15, calculator.Mul(5, 3));
        Assert.Equal(2, calculator.Div(6, 3));
    }

    [Fact]
    public void CreateCalculator_DivideByZero_ShouldThrowDivideByZeroException()
    {
        ICalculator calculator = CalculatorFactory.CreateCalculator();

        Assert.Throws<DivideByZeroException>(() => calculator.Div(10, 0));
    }

    [Fact]
    public void CreateCalculator_MultipleInstances_ShouldBeIndependentAndNotConflict()
    {
        ICalculator calc1 = CalculatorFactory.CreateCalculator();
        ICalculator calc2 = CalculatorFactory.CreateCalculator();

        Assert.NotSame(calc1, calc2);
        Assert.Equal(20, calc1.Add(10, 10));
        Assert.Equal(100, calc2.Mul(10, 10));
    }
}