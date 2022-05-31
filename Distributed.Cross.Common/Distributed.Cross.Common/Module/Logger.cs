using Distributed.Cross.Common.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Distributed.Cross.Common.Module
{
    public class Logger
    {
        public string Folder = "Log";
        public string LogFolder => Path.Combine(Const.ApplicationPath, Folder);
        public string LogPath => Path.Combine(Const.ApplicationPath, Folder, $"{Identifier}Log.txt");

        public string Identifier { get; set; }


        public Logger(string identifier)
        {
            Identifier = identifier;
            if (!Directory.Exists(LogFolder))
            {
                Directory.CreateDirectory(LogFolder);
            }
        }

        public void LogError(string error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            LogData(error, "ERR ");

        }

        public void LogWarning(string warning)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            LogData(warning, "WARN");
        }

        public void LogInformation(string information)
        {
            Console.ForegroundColor = ConsoleColor.White;
            LogData(information, "INFO");
        }

        public object LogLock = new object();

        private void LogData(string data, string type)
        {
            var log = $"{DateTime.Now:yyyy/MM/dd-hh:mm:ss:fff} | {type} | {Identifier} | {data}";
            Console.WriteLine(log);
            lock (LogLock)
            {
                File.AppendAllLines(LogPath, new[] { log });
            }

        }
    }
}
