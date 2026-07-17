using System;
using System.Collections.Concurrent;
using System.Threading;
using task17;
using Xunit;

namespace task17tests
{
    public class ServerThreadTests
    {
        private class TestCommand : ICommand
        {
            public bool WasExecuted { get; private set; } = false;

            public void Execute()
            {
                WasExecuted = true;
            }
        }

        [Fact]
        public void HardStop_ShouldStopImmediately_AndIgnoreRemainingCommands()
        {
            var queue = new BlockingCollection<ICommand>();
            var server = new ServerThread(queue);

            var cmd1 = new TestCommand();
            var hardStop = new HardStopCommand(server);
            var cmd2 = new TestCommand();

            queue.Add(cmd1);
            queue.Add(hardStop);
            queue.Add(cmd2);

            server.Start();
            server.Join();

            Assert.True(cmd1.WasExecuted);
            Assert.False(cmd2.WasExecuted);
        }

        [Fact]
        public void SoftStop_ShouldFinishAllQueuedCommandsBeforeStopping()
        {
            var queue = new BlockingCollection<ICommand>();
            var server = new ServerThread(queue);

            var cmd1 = new TestCommand();
            var softStop = new SoftStopCommand(server);
            var cmd2 = new TestCommand();

            queue.Add(cmd1);
            queue.Add(softStop);
            queue.Add(cmd2);

            server.Start();
            server.Join();

            Assert.True(cmd1.WasExecuted);
            Assert.True(cmd2.WasExecuted);
        }

        [Fact]
        public void HardStop_ShouldThrowException_WhenExecutedInWrongThread()
        {
            var queue = new BlockingCollection<ICommand>();
            var server = new ServerThread(queue);
            var hardStop = new HardStopCommand(server);

            Assert.Throws<InvalidOperationException>(() => hardStop.Execute());
        }

        [Fact]
        public void SoftStop_ShouldThrowException_WhenExecutedInWrongThread()
        {
            var queue = new BlockingCollection<ICommand>();
            var server = new ServerThread(queue);
            var softStop = new SoftStopCommand(server);

            Assert.Throws<InvalidOperationException>(() => softStop.Execute());
        }
    }
}