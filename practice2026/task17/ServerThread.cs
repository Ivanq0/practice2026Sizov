using System;
using System.Collections.Concurrent;
using System.Threading;

namespace task17
{
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
        private bool _stopRequested = false;

        public ServerThread(BlockingCollection<ICommand> queue)
        {
            _queue = queue;
            _thread = new Thread(Run);
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
                try
                {
                    ICommand cmd = _queue.Take();

                    try
                    {
                        cmd.Execute();
                    }
                    catch (Exception ex)
                    {
                        ExceptionHandler.Handle(cmd, ex);
                    }
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