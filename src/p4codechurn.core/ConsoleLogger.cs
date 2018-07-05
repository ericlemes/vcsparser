﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p4codechurn.core
{
    public class ConsoleLogger : ILogger
    {
        public void LogToConsole(string message)
        {
            Console.WriteLine(DateTime.Now.ToString("hh:mm:ss") + ": " + message);
        }
    }
}
