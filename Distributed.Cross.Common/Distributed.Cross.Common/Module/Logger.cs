using System;
using System.Collections.Generic;
using System.Text;

namespace Distributed.Cross.Common.Module
{
    public class Logger
    {
        public void LogError(string error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(error);
        }

        public void LogWarning(string warning)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(warning);
        }

        public void LogInformation(string information)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(information);
        }
    }
}
