using CommandLib;

namespace FileSystemCommands
{
    public class FindFilesCommand : ICommand
    {
        private readonly string _dirPath;
        private readonly string _mask;
        public List<string> FoundFiles { get; private set; } = new();

        public FindFilesCommand(string dirPath, string mask)
        {
            _dirPath = dirPath;
            _mask = mask;
        }

        public void Execute()
        {
            if (!Directory.Exists(_dirPath))
            {
                throw new DirectoryNotFoundException($"По указанному пути каталога не существует");
            }
            try
            {
                var files = Directory.GetFiles(_dirPath, _mask);
                FoundFiles = new List<string>(files);

                Console.WriteLine($"Количество найденных файлов: {files.Length}");
                foreach (var file in FoundFiles)
                {
                    Console.WriteLine(file);
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }
    }
}