using System;
using System.IO;
using System.Reflection;
using CommandLib;

namespace CommandRunner
{
    class CommandRunner
    {
        static void Main(string[] args)
        {
            string pluginPath = "C:/Users/vanek/Desktop/repo/practice2026/FileSystemCommands/bin/Debug/net8.0/FileSystemCommands.dll";
            Assembly assembly = Assembly.LoadFrom(pluginPath);

            var commandTypes = assembly.GetTypes().Where(type => typeof(ICommand).IsAssignableFrom(type) && type.IsClass).ToList();

            string testDir = Environment.CurrentDirectory;
            string testMask = "*.*";

            foreach (var type in commandTypes)
            {
                ICommand command = null;
                if (type.Name == "DirectorySizeCommand")
                {
                    command = (ICommand)Activator.CreateInstance(type, testDir);
                }
                else if (type.Name == "FindFilesCommand")
                {
                    command = (ICommand)Activator.CreateInstance(type, testDir, testMask);
                }
                command.Execute();
            }

            Console.ReadLine();
        }
    }
}