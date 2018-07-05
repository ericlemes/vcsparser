using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p4codechurn.core
{
    public class ProcessWrapper : IProcessWrapper
    {
        public Stream Invoke(string executable, string args)
        {
            var process = new Process();

            process.StartInfo.FileName = executable;
            process.StartInfo.Arguments = args;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;

            process.Start();
            //process.WaitForExit();
            
            return process.StandardOutput.BaseStream;
        }
    }
}
