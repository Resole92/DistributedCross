using System;
using System.Collections.Generic;
using System.Text;

namespace Distributed.Cross.Common.Module
{
    public class Logger
    {
        public string Identifier { get; set; }

        public Logger(string identifier)
        {
            Identifier = identifier;
        }

        public void LogError(string error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            LogData(error);

        }

        public void LogWarning(string warning)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            LogData(warning);
        }

        public void LogInformation(string information)
        {
            Console.ForegroundColor = ConsoleColor.White;
            LogData(information);
        }

        private void LogData(string data)
        {
            Console.WriteLine($"{DateTime.Now:yyyy/MM/dd-hh:mm:ss:fff} | {Identifier} | {data}");
        }
    }
}
