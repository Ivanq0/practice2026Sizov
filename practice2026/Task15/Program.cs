using System;
using System.Diagnostics;
using System.IO;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        double a = -100;
        double b = 100;
        Func<double, double> SIN = x => Math.Sin(x);
        double targetAccuracy = 1e-4;

        double[] steps = { 1e-1, 1e-2, 1e-3, 1e-4, 1e-5, 1e-6 };
        double chosenStep = 1e-3; // Теоретически обоснованный шаг

        // 1. Сбор данных по шагам для обоснования
        StringBuilder stepLog = new StringBuilder();
        foreach (var step in steps)
        {
            double res = DefiniteIntegral.SolveSingleThreaded(a, b, SIN, step);
            double error = Math.Abs(res - 0.0);
            stepLog.AppendLine($"  - Шаг {step:E1}: результат = {res:E1} (отклонение от точного значения ~ {error:E1})");
        }

        // 2. Замеры времени для потоков от 1 до 16 через Stopwatch
        int[] threadCounts = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
        double[] averageTimes = new double[threadCounts.Length];
        int iterations = 5;

        StringBuilder csvData = new StringBuilder();
        csvData.AppendLine("Threads,Time"); // Шапка для графика

        for (int i = 0; i < threadCounts.Length; i++)
        {
            int threads = threadCounts[i];
            double totalMs = 0;
            for (int j = 0; j < iterations; j++)
            {
                Stopwatch sw = Stopwatch.StartNew();
                DefiniteIntegral.Solve(a, b, SIN, chosenStep, threads);
                sw.Stop();
                totalMs += sw.Elapsed.TotalMilliseconds;
            }
            averageTimes[i] = totalMs / iterations;
            csvData.AppendLine($"{threads},{averageTimes[i]:F2}");
        }

        // Поиск лучшего времени
        int bestIdx = 0;
        for (int i = 1; i < threadCounts.Length; i++)
        {
            if (averageTimes[i] < averageTimes[bestIdx]) bestIdx = i;
        }

        // Замер чистого однопотока
        double singleThreadTotalMs = 0;
        for (int j = 0; j < iterations; j++)
        {
            Stopwatch sw = Stopwatch.StartNew();
            DefiniteIntegral.SolveSingleThreaded(a, b, SIN, chosenStep);
            sw.Stop();
            singleThreadTotalMs += sw.Elapsed.TotalMilliseconds;
        }
        double avgSingleTime = singleThreadTotalMs / iterations;

        int optimalThreads = threadCounts[bestIdx];
        double optimalMultiTime = averageTimes[bestIdx];
        double speedup = ((avgSingleTime - optimalMultiTime) / avgSingleTime) * 100.0;

        // 3. Формирование максимально простого отчета в report.txt
        StringBuilder report = new StringBuilder();
        report.AppendLine("=========================================================================");
        report.AppendLine("      ОТЧЕТ ПО ЛАБОРАТОРНОЙ РАБОТЕ: МНОГОПОТОЧНОЕ ИНТЕГРИРОВАНИЕ         ");
        report.AppendLine("=========================================================================\n");
        report.AppendLine($"Заданная точность: {targetAccuracy}");
        report.AppendLine($"Интервал интегрирования: [{a}, {b}]");
        report.AppendLine($"Функция: sin(x)\n");
        report.AppendLine("1. ОБОСНОВАНИЕ ВЫБОРА ШАГА");
        report.AppendLine("-------------------------------------------------------------------------");
        report.AppendLine("Фактические результаты расчетов при разных шагах:");
        report.Append(stepLog.ToString());
        report.AppendLine("\nВажно: При грубых шагах ошибка близка к 0 из-за симметрии интервала и нечетности синуса.");
        report.AppendLine("Поэтому шаг выбран по теоретической формуле погрешности метода трапеций.");
        report.AppendLine($"Выбран оптимальный шаг h = {chosenStep} (гарантирует погрешность < 1e-4).\n");
        report.AppendLine("2. РЕЗУЛЬТАТЫ ЗАМЕРОВ СКОРОСТИ (Stopwatch)");
        report.AppendLine("-------------------------------------------------------------------------");
        report.AppendLine($"Время работы в 1 поток: {avgSingleTime:F2} мс");
        report.AppendLine($"Оптимальное число фоновых потоков: {optimalThreads}");
        report.AppendLine($"Время работы в многопотоке: {optimalMultiTime:F2} мс");
        report.AppendLine($"Фактическое ускорение программы: {speedup:F2}%\n");
        report.AppendLine("Подробный график зависимости времени от числа потоков сохранен в threads_benchmark.png.");

        // Сохраняем текстовый отчет
        File.WriteAllText("report.txt", report.ToString(), Encoding.UTF8);

        // Сохраняем временные данные для графика
        File.WriteAllText("data.csv", csvData.ToString());

        // 4. Автоматическое построение графика силами Windows (через PowerShell)
        try
        {
            string psScript = @"
            $data = Import-Csv 'data.csv'
            Add-Type -AssemblyName System.Windows.Forms.DataVisualization
            $chart = New-Object System.Windows.Forms.DataVisualization.Charting.Chart
            $chart.Width = 600
            $chart.Height = 400
            $chartArea = New-Object System.Windows.Forms.DataVisualization.Charting.ChartArea
            $chart.ChartAreas.Add($chartArea)
            $series = New-Object System.Windows.Forms.DataVisualization.Charting.Series
            $series.ChartType = [System.Windows.Forms.DataVisualization.Charting.SeriesChartType]::Line
            $series.BorderWidth = 3
            foreach ($row in $data) { $series.Points.AddXY([int]$row.Threads, [double]$row.Time) }
            $chart.Series.Add($series)
            $chartArea.AxisX.Title = 'Количество потоков'
            $chartArea.AxisY.Title = 'Время выполнения (мс)'
            $chart.Titles.Add('Зависимость времени от потоков')
            $chart.SaveImage('threads_benchmark.png', [System.Windows.Forms.DataVisualization.Charting.ChartImageFormat]::Png)
            ";
            File.WriteAllText("plot.ps1", psScript);

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "powershell.exe";
            psi.Arguments = "-NoProfile -ExecutionPolicy Bypass -File plot.ps1";
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            Process.Start(psi)?.WaitForExit();

            // Чистим временные файлы
            if (File.Exists("data.csv")) File.Delete("data.csv");
            if (File.Exists("plot.ps1")) File.Delete("plot.ps1");
        }
        catch { /* Если на системе ограничены права, просто продолжим без падения */ }
    }
}