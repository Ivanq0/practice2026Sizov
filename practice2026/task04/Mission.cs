namespace task04;

public class Mission
{
    private readonly ISpaceship _spaceship;

    public Mission(ISpaceship spaceship)
    {
        _spaceship = spaceship;
    }

    public void StartMission()
    {
        _spaceship.MoveForward();
    }

    public void StartRotate()
    {
        _spaceship.Rotate(90);
    }

    public void StartAttack()
    {
        _spaceship.Fire();
    }
}