using System;
using System.Collections.Concurrent;
using System.Threading;
using task17;
using Xunit;

namespace task18
{
    public class StepwiseTestCommand : ILongRunningCommand
    {
        public int StepsDone { get; private set; } = 0;
        private readonly int _totalSteps;

        public bool IsCompleted => StepsDone >= _totalSteps;

        public StepwiseTestCommand(int totalSteps)
        {
            _totalSteps = totalSteps;
        }

        public void Execute()
        {
            if (!IsCompleted)
            {
                StepsDone++;
            }
        }
    }

    public class SimpleTestCommand : ICommand
    {
        public bool Executed { get; private set; } = false;
        public void Execute() => Executed = true;
    }

    public class SchedulerTests
    {
        [Fact]
        public void Test_LongRunningDecorator_CompletesAfterCorrectRuns()
        {
            var simpleCmd = new SimpleTestCommand();
            var longCmd = new LongRunningCommand(simpleCmd, 3);

            Assert.False(longCmd.IsCompleted);

            longCmd.Execute();
            longCmd.Execute();
            longCmd.Execute();

            Assert.True(longCmd.IsCompleted);
        }

        [Fact]
        public void Test_RoundRobin_SchedulesCorrectly()
        {
            var scheduler = new RoundRobinScheduler();
            var cmd1 = new StepwiseTestCommand(3);
            var cmd2 = new StepwiseTestCommand(3);

            scheduler.Add(cmd1);
            scheduler.Add(cmd2);

            var run1 = scheduler.Select();
            run1.Execute();
            scheduler.Add(cmd1);

            var run2 = scheduler.Select();
            run2.Execute();
            scheduler.Add(cmd2);

            Assert.Equal(1, cmd1.StepsDone);
            Assert.Equal(1, cmd2.StepsDone);
        }

        [Fact]
        public void Test_RoundRobin_InterleavesExecutionCorrectly()
        {
            var scheduler = new RoundRobinScheduler();

            var baseCmd1 = new SimpleTestCommand();
            var baseCmd2 = new SimpleTestCommand();

            var cmd1 = new LongRunningCommand(baseCmd1, 2);
            var cmd2 = new LongRunningCommand(baseCmd2, 2);

            scheduler.Add(cmd1);
            scheduler.Add(cmd2);

            var step1 = scheduler.Select();
            Assert.Same(cmd1, step1);
            step1.Execute();
            scheduler.Add(cmd1);

            var step2 = scheduler.Select();
            Assert.Same(cmd2, step2);
        }

        [Fact]
        public void Test_ServerThread_ProcessesStepwiseCommandsUntilCompleted()
        {
            var queue = new BlockingCollection<ICommand>();
            var scheduler = new RoundRobinScheduler();
            var server = new ServerThread(queue, scheduler);

            var longCmd = new StepwiseTestCommand(5);
            queue.Add(longCmd);
            queue.Add(new SoftStopCommand(server));

            server.Start();
            server.Join();

            Assert.True(longCmd.IsCompleted);
            Assert.Equal(5, longCmd.StepsDone);
        }

        [Fact]
        public void Test_HardStop_InterruptsLongRunningTasks()
        {
            var queue = new BlockingCollection<ICommand>();
            var scheduler = new RoundRobinScheduler();
            var server = new ServerThread(queue, scheduler);

            var endlessCmd = new StepwiseTestCommand(1000);

            queue.Add(endlessCmd);
            queue.Add(new HardStopCommand(server));

            server.Start();
            server.Join();

            Assert.False(endlessCmd.IsCompleted);
            Assert.True(endlessCmd.StepsDone < 1000);
        }

        [Fact]
        public void Test_Security_StopCommandsThrowExceptionInWrongThread()
        {
            var queue = new BlockingCollection<ICommand>();
            var scheduler = new RoundRobinScheduler();
            var server = new ServerThread(queue, scheduler);

            var hardStop = new HardStopCommand(server);
            var softStop = new SoftStopCommand(server);

            Assert.Throws<InvalidOperationException>(() => hardStop.Execute());
            Assert.Throws<InvalidOperationException>(() => softStop.Execute());
        }
    }
}