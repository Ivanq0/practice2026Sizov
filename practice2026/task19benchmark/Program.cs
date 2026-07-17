using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using task17;

namespace task19
{
    public class BenchmarkCommand : ILongRunningCommand
    {
        private readonly int _totalWork;
        private readonly int _workPerQuantum;
        private int _completedWork = 0;

        public bool IsCompleted => _completedWork >= _totalWork;

        public BenchmarkCommand(int totalWork, int workPerQuantum)
        {
            _totalWork = totalWork;
            _workPerQuantum = workPerQuantum;
        }

        public void Execute()
        {
            int currentStepSize = Math.Min(_workPerQuantum, _totalWork - _completedWork);
            double dummyVal = 0;

            for (int i = 0; i < currentStepSize; i++)
            {
                dummyVal += Math.Sin(i) * Math.Cos(i);
            }

            _completedWork += currentStepSize;
        }
    }

    public class Program
    {
        public static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("=== Запуск демонстрации ЛР №19 (Длительные операции) ===");

            Console.WriteLine("\n[1] Иллюстрация выполнения 5 экземпляров TestCommand по 3 раза:");
            RunTzIllustration();

            Console.WriteLine("\n[2] Запуск бенчмарка накладных расходов планировщика...");
            RunPerformanceBenchmark();
        }

        private static void RunTzIllustration()
        {
            var queue = new BlockingCollection<task17.ICommand>();
            var scheduler = new RoundRobinScheduler();
            var server = new ServerThread(queue, scheduler);

            var commands = new LongRunningCommand[5];
            for (int i = 0; i < 5; i++)
            {
                var baseCmd = new TestCommand(i + 1);
                commands[i] = new LongRunningCommand(baseCmd, 3);
                queue.Add(commands[i]);
            }

            server.Start();

            bool allFinished = false;
            while (!allFinished)
            {
                allFinished = true;
                foreach (var cmd in commands)
                {
                    if (!cmd.IsCompleted)
                    {
                        allFinished = false;
                        break;
                    }
                }
                Thread.Sleep(10);
            }

            Console.WriteLine("\n[Иллюстрация] Все 5 команд успешно завершили по 3 шага.");
            Console.WriteLine("[Иллюстрация] Отправляем HardStop для остановки потока...");

            queue.Add(new HardStopCommand(server));
            server.Join();

            Console.WriteLine("[Иллюстрация] Поток сервера успешно остановлен.");
        }

        private static void RunPerformanceBenchmark()
        {
            const int totalWorkLoad = 1000000;
            int[] quantums = { 10, 50, 250, 1250, 6250, 31250 };
            double[] times = new double[quantums.Length];

            Console.WriteLine($"\nОбщая нагрузка: {totalWorkLoad} операций.");
            Console.WriteLine("Измеряем накладные расходы планировщика при разной частоте переключений задач:");

            for (int i = 0; i < quantums.Length; i++)
            {
                int q = quantums[i];
                var queue = new BlockingCollection<task17.ICommand>();
                var scheduler = new RoundRobinScheduler();
                var server = new ServerThread(queue, scheduler);

                for (int t = 0; t < 4; t++)
                {
                    queue.Add(new BenchmarkCommand(totalWorkLoad / 4, q));
                }

                Stopwatch sw = Stopwatch.StartNew();
                server.Start();

                while (scheduler.HasCommand() || queue.Count > 0)
                {
                    Thread.Sleep(5);
                }

                queue.Add(new HardStopCommand(server));
                server.Join();
                sw.Stop();

                times[i] = sw.Elapsed.TotalMilliseconds;
                Console.WriteLine($" - Квант: {q,5} оп/вызов | Время: {times[i]:F2} мс");
            }

            GenerateTextReport(quantums, times);
            GenerateSvgGraph(quantums, times);
        }

        private static void GenerateSvgGraph(int[] quantums, double[] times)
        {
            try
            {
                double minTime = times[0];
                double maxTime = times[0];
                foreach (var t in times)
                {
                    if (t < minTime) minTime = t;
                    if (t > maxTime) maxTime = t;
                }

                double margin = (maxTime - minTime) * 0.1;
                if (margin < 1.0) margin = 1.0;
                double scaleMin = Math.Max(0, minTime - margin);
                double scaleMax = maxTime + margin;

                StringBuilder svg = new StringBuilder();
                svg.AppendLine("<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 700 450\" width=\"100%\" height=\"100%\" style=\"background-color:#1e1e24; font-family:Segoe UI, sans-serif;\">");

                svg.AppendLine("  <defs>");
                svg.AppendLine("    <linearGradient id=\"lineGrad\" x1=\"0%\" y1=\"0%\" x2=\"100%\" y2=\"100%\">");
                svg.AppendLine("      <stop offset=\"0%\" stop-color=\"#818cf8\" />");
                svg.AppendLine("      <stop offset=\"100%\" stop-color=\"#ec4899\" />");
                svg.AppendLine("    </linearGradient>");
                svg.AppendLine("    <linearGradient id=\"areaGrad\" x1=\"0%\" y1=\"0%\" x2=\"0%\" y2=\"100%\">");
                svg.AppendLine("      <stop offset=\"0%\" stop-color=\"#818cf8\" stop-opacity=\"0.4\" />");
                svg.AppendLine("      <stop offset=\"100%\" stop-color=\"#818cf8\" stop-opacity=\"0.0\" />");
                svg.AppendLine("    </linearGradient>");
                svg.AppendLine("  </defs>");

                svg.AppendLine("  <text x=\"350\" y=\"35\" fill=\"#f3f4f6\" font-size=\"18\" font-weight=\"bold\" text-anchor=\"middle\">Зависимость времени от размера кванта вычислений (ЛР 19)</text>");

                double xLeft = 80, xRight = 640;
                double yTop = 70, yBottom = 380;
                double width = xRight - xLeft;
                double height = yBottom - yTop;

                int gridLines = 5;
                for (int i = 0; i <= gridLines; i++)
                {
                    double y = yBottom - (i * (height / gridLines));
                    double val = scaleMin + (i * ((scaleMax - scaleMin) / gridLines));
                    svg.AppendLine($"  <line x1=\"{xLeft}\" y1=\"{y}\" x2=\"{xRight}\" y2=\"{y}\" stroke=\"#4b5563\" stroke-dasharray=\"4,4\" stroke-width=\"1\" />");
                    svg.AppendLine($"  <text x=\"{xLeft - 10}\" y=\"{y + 4}\" fill=\"#9ca3af\" font-size=\"11\" text-anchor=\"end\">{val:F1} мс</text>");
                }

                double[] xCoords = new double[quantums.Length];
                double[] yCoords = new double[quantums.Length];
                for (int i = 0; i < quantums.Length; i++)
                {
                    xCoords[i] = xLeft + (i * (width / (quantums.Length - 1)));

                    double ratio = (times[i] - scaleMin) / (scaleMax - scaleMin);
                    if (double.IsNaN(ratio)) ratio = 0.5;
                    yCoords[i] = yBottom - (ratio * height);
                }

                StringBuilder areaPath = new StringBuilder();
                areaPath.Append($"M {xCoords[0]} {yBottom} ");
                for (int i = 0; i < quantums.Length; i++)
                {
                    areaPath.Append($"L {xCoords[i]} {yCoords[i]} ");
                }
                areaPath.Append($"L {xCoords[quantums.Length - 1]} {yBottom} Z");
                svg.AppendLine($"  <path d=\"{areaPath}\" fill=\"url(#areaGrad)\" />");

                StringBuilder linePath = new StringBuilder();
                linePath.Append($"M {xCoords[0]} {yCoords[0]} ");
                for (int i = 1; i < quantums.Length; i++)
                {
                    linePath.Append($"L {xCoords[i]} {yCoords[i]} ");
                }
                svg.AppendLine($"  <path d=\"{linePath}\" fill=\"none\" stroke=\"url(#lineGrad)\" stroke-width=\"4\" stroke-linecap=\"round\" stroke-linejoin=\"round\" />");

                for (int i = 0; i < quantums.Length; i++)
                {
                    svg.AppendLine($"  <circle cx=\"{xCoords[i]}\" cy=\"{yCoords[i]}\" r=\"6\" fill=\"#1e1e24\" stroke=\"#ec4899\" stroke-width=\"3\" />");
                    double textYOffset = -12;
                    svg.AppendLine($"  <text x=\"{xCoords[i]}\" y=\"{yCoords[i] + textYOffset}\" fill=\"#f3f4f6\" font-size=\"11\" font-weight=\"bold\" text-anchor=\"middle\">{times[i]:F1} мс</text>");
                    svg.AppendLine($"  <text x=\"{xCoords[i]}\" y=\"{yBottom + 20}\" fill=\"#9ca3af\" font-size=\"11\" text-anchor=\"middle\">q = {quantums[i]}</text>");
                }

                svg.AppendLine($"  <text x=\"350\" y=\"430\" fill=\"#f3f4f6\" font-size=\"12\" text-anchor=\"middle\">Размер кванта вычислений (кол-во операций на один шаг Execute)</text>");
                svg.AppendLine($"  <text x=\"20\" y=\"225\" fill=\"#f3f4f6\" font-size=\"12\" text-anchor=\"middle\" transform=\"rotate(-90 20 225)\">Время вычислений (мс)</text>");

                svg.AppendLine("</svg>");

                File.WriteAllText("scheduler_benchmark.svg", svg.ToString(), Encoding.UTF8);
                Console.WriteLine("\n[Успех] Превосходный векторный график сохранен как: 'scheduler_benchmark.svg'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Внимание] Ошибка рендеринга SVG: {ex.Message}");
            }
        }

        private static void GenerateTextReport(int[] quantums, double[] times)
        {
            StringBuilder report = new StringBuilder();
            report.AppendLine("=========================================================================");
            report.AppendLine("      ОТЧЕТ ПО ЛАБОРАТОРНОЙ РАБОТЕ №19: РЕАЛИЗАЦИЯ ДЛИТЕЛЬНЫХ ОПЕРАЦИЙ   ");
            report.AppendLine("=========================================================================\n");
            report.AppendLine("1. ИЛЛЮСТРАЦИЯ ПСЕВДОПАРАЛЛЕЛЬНОЙ РАБОТЫ (согласно ТЗ)");
            report.AppendLine("-------------------------------------------------------------------------");
            report.AppendLine("В рамках работы была реализована команда TestCommand(id) с первичным конструктором C# 12.");
            report.AppendLine("Для ее корректного встраивания в планировщик без изменения оригинального кода");
            report.AppendLine("был применен паттерн проектирования 'Декоратор' (класс LongRunningCommand).");
            report.AppendLine("Это позволило запустить 5 задач параллельно. Результаты работы:");
            report.AppendLine(" - Поток переключается между задачами после выполнения каждого шага (кванта).");
            report.AppendLine(" - Остановка потока произведена безопасно с помощью HardStopCommand.");
            report.AppendLine(" - Исключена монополизация потока одной задачей.\n");
            report.AppendLine("2. ЗАВИСИМОСТЬ ЭФФЕКТИВНОСТИ ВЫЧИСЛЕНИЙ ОТ РАЗМЕРА КВАНТА (Бенчмарк)");
            report.AppendLine("-------------------------------------------------------------------------");
            report.AppendLine("При уменьшении кванта (дробления на мелкие шаги) увеличивается интерактивность");
            report.AppendLine("системы, однако растут накладные расходы на планирование.");
            report.AppendLine("Результаты тестирования на 1 000 000 вычислений при 4 параллельных задачах:");

            for (int i = 0; i < quantums.Length; i++)
            {
                report.AppendLine($" - Размер кванта: {quantums[i],5} операций | Общее время выполнения: {times[i]:F2} мс");
            }

            File.WriteAllText("report_task19.txt", report.ToString(), Encoding.UTF8);
            Console.WriteLine("[Успех] Текстовый отчет сохранен в: 'report_task19.txt'");
        }
    }
}