using CommandLib;
using System.IO;

namespace FileSystemCommands
{
    public class DirectorySizeCommand : ICommand
    {
        private string _dirPath;
        public DirectorySizeCommand(string dirPath)
        {
            _dirPath = dirPath;
        }

        public void Execute()
        {
            var dirInfo = new DirectoryInfo(_dirPath);
            long size = dirInfo.EnumerateFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);
            Console.WriteLine($"Размер {_dirPath} - {size} байт");
        }
    }
    public class FindFilesCommand : ICommand
    {
        private readonly string _dirPath;
        private readonly string _mask;

        public FindFilesCommand(string dirPath, string mask)
        {
            _dirPath = dirPath;
            _mask = mask;
        }

        public void Execute()
        {
            var files = Directory.GetFiles(_dirPath, _mask);

            Console.WriteLine($"Количество найденных файлов: {files.Length}");
            foreach (var file in files)
            {
                Console.WriteLine(file);
            }
        }
    }
}

