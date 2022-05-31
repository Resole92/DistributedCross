using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Distributed.Cross.Common.Utilities
{
    public class Const
    {
        public static TimeSpan MaxTimeout => TimeSpan.FromSeconds(1.5);
        public static int BrokenIdentifier => 0;
        public static int EnvironmentIdentifier => -1;

        public static string ApplicationPath { get; private set; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    }
}
