using System;
using System.Threading;

public class DefiniteIntegral
{
    public static double TotalResult = 0;

    public static double Solve(double a, double b, Func<double, double> function, double step, int threadsnumber)
    { 
        TotalResult = 0;

        Barrier myBarrier = new Barrier(threadsnumber + 1);
        double chunkLength = (b - a) / threadsnumber;
        Thread[] myThreads = new Thread[threadsnumber];

        for (int i = 0; i < threadsnumber; i++)
        {
            int threadId = i;
            myThreads[threadId] = new Thread(() =>
            {
                double myStart = a + threadId * chunkLength;
                double myEnd = myStart + chunkLength;
                double localSum = 0;

                double x = myStart;
                while (x < myEnd)
                {
                    double nextX = x + step;

                    if (nextX > myEnd)
                    {
                        nextX = myEnd;
                    }

                    double h = nextX - x;
                    double y1 = function(x);
                    double y2 = function(nextX);

                    localSum += h * (y1 + y2) / 2.0;

                    x = nextX;
                }

                double oldTotal;
                do
                {
                    oldTotal = TotalResult;
                }
                while (Interlocked.CompareExchange(ref TotalResult, oldTotal + localSum, oldTotal) != oldTotal);

                myBarrier.SignalAndWait();
            });
            myThreads[threadId].Start();
        }
        myBarrier.SignalAndWait();

        return TotalResult;
    }
}