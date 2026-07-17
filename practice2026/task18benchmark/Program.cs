using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace task17
{
    public class HeavyCalculationCommand : ILongRunningCommand
    {
        private readonly int _totalSteps;
        private readonly int _quantum;
        private int _currentStep = 0;

        public bool IsCompleted => _currentStep >= _totalSteps;

        public HeavyCalculationCommand(int totalSteps, int quantum)
        {
            _totalSteps = totalSteps;
            _quantum = quantum;
        }

        public void Execute()
        {
            if (IsCompleted) return;

            int stepsToExecute = Math.Min(_quantum, _totalSteps - _currentStep);

            double temp = 0;
            for (int i = 0; i < stepsToExecute; i++)
            {
                temp += Math.Sin(i) * Math.Cos(i);
                _currentStep++;
            }
        }
    }
    public class Program
    {
        public static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("=== Запуск бенчмарка для планировщика команд (Задача №18) ===");

            int[] quantums = { 5, 50, 500, 5000, 50000, 500000 };
            double[] executionTimes = new double[quantums.Length];

            int totalStepsPerJob = 1000000;
            int numberOfJobs = 10;
            int runs = 3;

            StringBuilder csvData = new StringBuilder();
            csvData.AppendLine("Quantum,Time");

            for (int q = 0; q < quantums.Length; q++)
            {
                int currentQuantum = quantums[q];
                double totalMs = 0;

                Console.WriteLine($"Тестирование кванта: {currentQuantum,7} шагов...");

                for (int run = 0; run < runs; run++)
                {
                    var queue = new BlockingCollection<ICommand>();
                    var scheduler = new RoundRobinScheduler();
                    var server = new ServerThread(queue, scheduler);

                    for (int j = 0; j < numberOfJobs; j++)
                    {
                        queue.Add(new HeavyCalculationCommand(totalStepsPerJob, currentQuantum));
                    }

                    queue.Add(new SoftStopCommand(server));

                    Stopwatch sw = Stopwatch.StartNew();
                    server.Start();
                    server.Join();
                    sw.Stop();

                    totalMs += sw.Elapsed.TotalMilliseconds;
                }

                executionTimes[q] = totalMs / runs;
                csvData.AppendLine($"{currentQuantum},{executionTimes[q]:F2}");
                Console.WriteLine($" -> Среднее время выполнения: {executionTimes[q]:F2} мс\n");
            }
            GenerateTextReport(quantums, executionTimes);

            GenerateSvgChart(quantums, executionTimes);
        }

        private static void GenerateTextReport(int[] quantums, double[] executionTimes)
        {
            StringBuilder report = new StringBuilder();
            report.AppendLine("=========================================================================");
            report.AppendLine("           ОТЧЕТ ПО ЛАБОРАТОРНОЙ РАБОТЕ №18: ПЛАНИРОВЩИК КОМАНД          ");
            report.AppendLine("=========================================================================\n");
            report.AppendLine("1. ОБОСНОВАНИЕ АРХИТЕКТУРЫ");
            report.AppendLine("-------------------------------------------------------------------------");
            report.AppendLine("Для предотвращения взаимной блокировки (Deadlock) и обеспечения");
            report.AppendLine("псевдопараллельного выполнения тяжелых задач была реализована стратегия");
            report.AppendLine("Round Robin в планировщике.");
            report.AppendLine("Поток ServerThread эффективно совмещает неблокирующий опрос внешней очереди (TryTake)");
            report.AppendLine("с обработкой квантов задач из планировщика.");
            report.AppendLine("При простое системы (когда нет задач) поток засыпает на блокирующем Take(),");
            report.AppendLine("что снижает нагрузку на CPU до 0%.\n");
            report.AppendLine("2. ЗАВИСИМОСТЬ ПРОИЗВОДИТЕЛЬНОСТИ ОТ РАЗМЕРА КВАНТА");
            report.AppendLine("-------------------------------------------------------------------------");
            for (int i = 0; i < quantums.Length; i++)
            {
                report.AppendLine($"  - Квант {quantums[i],7} шагов: среднее время выполнения = {executionTimes[i]:F2} мс");
            }

            File.WriteAllText("report_task18.txt", report.ToString(), Encoding.UTF8);
            Console.WriteLine("[Успех] Текстовый отчет сохранен в 'report_task18.txt'.");
        }

        private static void GenerateSvgChart(int[] quantums, double[] executionTimes)
        {
            try
            {
                // Константы размеров SVG холста
                int width = 800;
                int height = 500;
                int padLeft = 90;
                int padRight = 50;
                int padTop = 60;
                int padBottom = 70;

                int chartWidth = width - padLeft - padRight;
                int chartHeight = height - padTop - padBottom;

                // Нахождение экстремумов по оси Y
                double maxTime = 0;
                foreach (var t in executionTimes) if (t > maxTime) maxTime = t;

                // Делаем шкалу округленной вверх с небольшим запасом
                double maxYAxis = Math.Ceiling(maxTime * 1.15 / 10.0) * 10.0;
                if (maxYAxis <= 0) maxYAxis = 100;

                StringBuilder svg = new StringBuilder();
                svg.AppendLine($"<svg width=\"{width}\" height=\"{height}\" viewBox=\"0 0 {width} {height}\" xmlns=\"http://www.w3.org/2000/svg\" style=\"background:#ffffff; font-family:'Segoe UI', -apple-system, sans-serif;\">");

                // 1. Стили и градиенты для эффекта современного UI (как на дашбордах)
                svg.AppendLine("  <defs>");
                svg.AppendLine("    <linearGradient id=\"areaGrad\" x1=\"0\" y1=\"0\" x2=\"0\" y2=\"1\">");
                svg.AppendLine("      <stop offset=\"0%\" stop-color=\"#4f46e5\" stop-opacity=\"0.3\"/>");
                svg.AppendLine("      <stop offset=\"100%\" stop-color=\"#4f46e5\" stop-opacity=\"0.0\"/>");
                svg.AppendLine("    </linearGradient>");
                svg.AppendLine("  </defs>");

                // 2. Сетка по оси Y (горизонтальные линии и подписи)
                int gridLinesCount = 5;
                for (int i = 0; i <= gridLinesCount; i++)
                {
                    double val = (maxYAxis / gridLinesCount) * i;
                    double y = (height - padBottom) - (val / maxYAxis) * chartHeight;

                    // Рисуем неяркую линию сетки
                    svg.AppendLine($"  <line x1=\"{padLeft}\" y1=\"{y}\" x2=\"{width - padRight}\" y2=\"{y}\" stroke=\"#f1f5f9\" stroke-width=\"1\" />");
                    // Рисуем подпись
                    svg.AppendLine($"  <text x=\"{padLeft - 15}\" y=\"{y + 4}\" text-anchor=\"end\" font-size=\"11\" fill=\"#64748b\">{val:F0}</text>");
                }

                // 3. Вычисление экранных координат точек
                double[] xCoords = new double[quantums.Length];
                double[] yCoords = new double[quantums.Length];

                for (int i = 0; i < quantums.Length; i++)
                {
                    // Располагаем точки логарифмически равномерно (так как шаг растет в 10 раз: 5 -> 50 -> 500...)
                    xCoords[i] = padLeft + i * ((double)chartWidth / (quantums.Length - 1));
                    yCoords[i] = (height - padBottom) - (executionTimes[i] / maxYAxis) * chartHeight;
                }

                // 4. Построение фонового градиента под графиком (Area chart effect)
                StringBuilder areaPath = new StringBuilder();
                areaPath.Append($"M {xCoords[0]} {height - padBottom} ");
                for (int i = 0; i < quantums.Length; i++)
                {
                    areaPath.Append($"L {xCoords[i]} {yCoords[i]} ");
                }
                areaPath.Append($"L {xCoords[quantums.Length - 1]} {height - padBottom} Z");
                svg.AppendLine($"  <path d=\"{areaPath}\" fill=\"url(#areaGrad)\" />");

                // 5. Построение основной соединительной линии
                StringBuilder linePath = new StringBuilder();
                linePath.Append($"M {xCoords[0]} {yCoords[0]} ");
                for (int i = 1; i < quantums.Length; i++)
                {
                    linePath.Append($"L {xCoords[i]} {yCoords[i]} ");
                }
                svg.AppendLine($"  <path d=\"{linePath}\" fill=\"none\" stroke=\"#4f46e5\" stroke-width=\"3.5\" stroke-linecap=\"round\" stroke-linejoin=\"round\" />");

                // 6. Рисование точек (узлов) и подписей по оси X (размер кванта)
                for (int i = 0; i < quantums.Length; i++)
                {
                    // Точка
                    svg.AppendLine($"  <circle cx=\"{xCoords[i]}\" cy=\"{yCoords[i]}\" r=\"5.5\" fill=\"#ffffff\" stroke=\"#4f46e5\" stroke-width=\"3\" />");

                    // Значение времени над точкой
                    svg.AppendLine($"  <text x=\"{xCoords[i]}\" y=\"{yCoords[i] - 12}\" text-anchor=\"middle\" font-size=\"11\" font-weight=\"bold\" fill=\"#1e293b\">{executionTimes[i]:F1} мс</text>");

                    // Подпись оси X
                    svg.AppendLine($"  <text x=\"{xCoords[i]}\" y=\"{height - padBottom + 25}\" text-anchor=\"middle\" font-size=\"11\" font-weight=\"500\" fill=\"#64748b\">{quantums[i]}</text>");
                }

                // 7. Подписи осей и заголовок
                // Заголовок
                svg.AppendLine($"  <text x=\"{width / 2}\" y=\"30\" text-anchor=\"middle\" font-size=\"16\" font-weight=\"bold\" fill=\"#0f172a\">Влияние размера кванта на общую производительность</text>");
                // Подпись X
                svg.AppendLine($"  <text x=\"{width / 2}\" y=\"{height - 15}\" text-anchor=\"middle\" font-size=\"12\" font-weight=\"600\" fill=\"#334155\">Размер вычислительного кванта (число шагов в Execute)</text>");
                // Подпись Y
                svg.AppendLine($"  <text x=\"25\" y=\"{height / 2}\" text-anchor=\"middle\" font-size=\"12\" font-weight=\"600\" fill=\"#334155\" transform=\"rotate(-90 25 {height / 2})\">Время работы планировщика (мс)</text>");

                // Закрываем тег
                svg.AppendLine("</svg>");

                // Сохранение файла
                File.WriteAllText("scheduler_benchmark.svg", svg.ToString(), Encoding.UTF8);
                Console.WriteLine("[Успех] График успешно построен и сохранен как 'scheduler_benchmark.svg'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Ошибка] Не удалось сформировать SVG график: {ex.Message}");
            }
        }
    }
}
