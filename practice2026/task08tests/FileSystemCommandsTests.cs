using FileSystemCommands;

namespace task08tests
{
    public class FileSystemCommandsTests
    {
        [Fact]
        public void DirectorySizeCommand_ShouldCalculateSize()
        {   
            var testDir = Path.Combine(Path.GetTempPath(), "TestDir");
            Directory.CreateDirectory(testDir);
            File.WriteAllText(Path.Combine(testDir, "test1.txt"), "Hello");
            File.WriteAllText(Path.Combine(testDir, "test2.txt"), "World");

            var command = new DirectorySizeCommand(testDir);
            command.Execute();

            Directory.Delete(testDir, true);
        }

        [Fact]
        public void FindFilesCommand_ShouldFindMatchingFiles()
        {
            var testDir = Path.Combine(Path.GetTempPath(), "TestDir");
            Directory.CreateDirectory(testDir);
            File.WriteAllText(Path.Combine(testDir, "file1.txt"), "Text");
            File.WriteAllText(Path.Combine(testDir, "file2.log"), "Log");

            var command = new FindFilesCommand(testDir, "*.txt");
            command.Execute();

            Directory.Delete(testDir, true);
        }

        [Fact]
        public void DirectorySizeCommand_ShouldThrowWhenDirectoryDoesNotExist_TryCatch()
        {
            var fake_path = Path.Combine(Path.GetTempPath(), "NotExistDirectory");
            var command = new DirectorySizeCommand(fake_path);
            try
            {
                command.Execute();
                Assert.Fail("Исключение DirectoryNotFoundException");
            }
            catch (DirectoryNotFoundException) { }
        }

        [Fact]
        public void FindFilesCommand_ShouldReturnEmptyListWhenNoFilesMatchMask_TryCatch()
        {
            var test_directory_path = Path.Combine(Path.GetTempPath(), "TestDirectory");
            if (Directory.Exists(test_directory_path)) Directory.Delete(test_directory_path, true);
            Directory.CreateDirectory(test_directory_path);
            var command = new FindFilesCommand(test_directory_path, "*.mp3");
            try
            {
                command.Execute();
                Assert.Empty(command.FoundFiles);
            }
            catch (Exception ex) { Assert.Fail($"Исключение {ex.Message}"); }
            finally
            {
                if (Directory.Exists(test_directory_path)) Directory.Delete(test_directory_path, true);
            }
        }
    }
}