using Moq;
using task04;

public class SpaceshipTests
{
    [Fact]
    public void Cruiser_ShouldHaveCorrectStats()
    {
        ISpaceship cruiser = new Cruiser();
        Assert.Equal(50, cruiser.Speed);
        Assert.Equal(100, cruiser.FirePower);
    }

    [Fact]
    public void Fighter_ShouldHaveCorrectStats()
    {
        ISpaceship fighter = new Fighter();
        Assert.Equal(100, fighter.Speed);
        Assert.Equal(50, fighter.FirePower);
    }

    [Fact]
    public void Fighter_ShouldBeFasterThanCruiser()
    {
        var fighter = new Fighter();
        var cruiser = new Cruiser();
        Assert.True(fighter.Speed > cruiser.Speed);
    }

    [Fact]
    public void Cruiser_ShouldBeStrongerThanFighter()
    {
        var fighter = new Fighter();
        var cruiser = new Cruiser();
        Assert.True(fighter.FirePower < cruiser.FirePower);
    }

    [Fact]
    public void Cruiser_Actions_DoesNotThrowException()
    {
        var cruiser = new Cruiser();
        var exception_move = Record.Exception(() => cruiser.MoveForward());
        var exception_rotate = Record.Exception(() => cruiser.Rotate(90));
        var exception_fire = Record.Exception(() => cruiser.Fire());
        Assert.Null(exception_move);
        Assert.Null(exception_rotate);
        Assert.Null(exception_fire);
    }

    [Fact]
    public void Fighter_Actions_DoesNotThrowException()
    {
        var fighter = new Fighter();
        var exception_move = Record.Exception(() => fighter.MoveForward());
        var exception_rotate = Record.Exception(() => fighter.Rotate(90));
        var exception_fire = Record.Exception(() => fighter.Fire());
        Assert.Null(exception_move);
        Assert.Null(exception_rotate);
        Assert.Null(exception_fire);
    }
}