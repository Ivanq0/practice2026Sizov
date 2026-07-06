using CommandLib;

namespace FileSystemCommands
{
    public class DirectorySizeCommand : ICommand
    {
        private string _dirPath;
        public DirectorySizeCommand(string dirPath)
        {
            if (string.IsNullOrWhiteSpace(dirPath))
            {
                throw new ArgumentException("Путь к директории не может быть пустым.", nameof(dirPath));
            }
            _dirPath = dirPath;
        }

        public void Execute()
        {
            if (!Directory.Exists(_dirPath))
            {
                throw new DirectoryNotFoundException($"Директория по пути '{_dirPath}' не найдена.");
            }
            var dirInfo = new DirectoryInfo(_dirPath);
            long size = dirInfo.EnumerateFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);
            Console.WriteLine($"Размер {_dirPath} - {size} байт");
        }
    }
}
