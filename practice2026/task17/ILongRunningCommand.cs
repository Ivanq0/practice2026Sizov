namespace task17
{
    public interface ILongRunningCommand : ICommand
    {
        bool IsCompleted { get; }
    }
}
