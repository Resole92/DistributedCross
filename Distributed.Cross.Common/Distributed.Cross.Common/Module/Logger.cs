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
            Console.WriteLine(error);
        }

        public void LogWarning(string warning)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(warning);
        }

        public void LogInformation(string information)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(information);
        }
    }
}
