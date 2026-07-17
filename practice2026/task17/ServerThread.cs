using System;
using System.Collections.Concurrent;
using System.Threading;

namespace task17
{
    public class RoundRobinScheduler : IScheduler
    {
        private readonly Queue<ICommand> _jobs = new Queue<ICommand>();

        public bool HasCommand() => _jobs.Count > 0;

        public ICommand Select()
        {
            if (_jobs.Count == 0) return null;
            return _jobs.Dequeue();
        }

        public void Add(ICommand cmd)
        {
            if (cmd != null)
            {
                _jobs.Enqueue(cmd);
            }
        }
    }

    public static class ExceptionHandler
    {
        public static void Handle(ICommand cmd, Exception ex)
        {
            Console.WriteLine($"[Ошибка] Команда {cmd.GetType().Name} завершилась с ошибкой: {ex.Message}");
        }
    }

    public class ServerThread
    {
        private readonly BlockingCollection<ICommand> _queue;
        private readonly Thread _thread;
        private readonly IScheduler _scheduler;
        private bool _stopRequested = false;

        public ServerThread(BlockingCollection<ICommand> queue, IScheduler scheduler)
        {
            _queue = queue;
            _thread = new Thread(Run);
            _scheduler = scheduler;
        }

        public void Start() => _thread.Start();

        public void Join() => _thread.Join();

        public Thread GetInternalThread() => _thread;

        public void StopLoop()
        {
            _stopRequested = true;
        }

        public void SoftStop()
        {
            _queue.CompleteAdding();
        }

        private void Run()
        {
            while (!_stopRequested)
            {
                bool hasLocalWork = _scheduler.HasCommand();
                if (hasLocalWork)
                {
                    while (_queue.TryTake(out ICommand newCmd))
                    {
                        _scheduler.Add(newCmd);
                    }

                    ICommand cmd = _scheduler.Select();
                    try
                    {
                        cmd.Execute();

                        if (cmd is ILongRunningCommand longCmd && !longCmd.IsCompleted)
                        {
                            _scheduler.Add(longCmd);
                        }
                    }
                    catch (Exception ex)
                    {
                        ExceptionHandler.Handle(cmd, ex);
                    }
                }
                else
                {
                    if (_queue.IsCompleted)
                    {
                        break;
                    }

                    try
                    {
                        ICommand cmd = _queue.Take();
                        _scheduler.Add(cmd);
                    }
                    catch (InvalidOperationException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Критическая ошибка в потоке: {ex.Message}");
                    }
                }
            }
        }
    }

    public class HardStopCommand : ICommand
    {
        private readonly ServerThread _serverThread;

        public HardStopCommand(ServerThread thread)
        {
            _serverThread = thread;
        }

        public void Execute()
        {
            if (Thread.CurrentThread != _serverThread.GetInternalThread())
            {
                throw new InvalidOperationException("HardStop должен запускаться только внутри самого ServerThread.");
            }
            _serverThread.StopLoop();
        }
    }

    public class SoftStopCommand : ICommand
    {
        private readonly ServerThread _serverThread;

        public SoftStopCommand(ServerThread thread)
        {
            _serverThread = thread;
        }

        public void Execute()
        {
            if (Thread.CurrentThread != _serverThread.GetInternalThread())
            {
                throw new InvalidOperationException("SoftStop должен запускаться только внутри самого ServerThread.");
            }
            _serverThread.SoftStop();
        }
    }
}