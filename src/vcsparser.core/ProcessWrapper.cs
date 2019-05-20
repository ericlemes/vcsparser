using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core
{
    public class ProcessWrapper : IProcessWrapper
    {
        private Process CreateProcessWithBaseParams(string executable, string args)
        {
            var process = new Process();

            process.StartInfo.FileName = executable;
            process.StartInfo.Arguments = args;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;

            return process;
        }

        public Stream Invoke(string executable, string args)
        {
            var process = CreateProcessWithBaseParams(executable, args);

            process.Start();                        
            return process.StandardOutput.BaseStream;
        }        

        public Stream Invoke(string executable, string args, string workingDir)
        {
            var process = CreateProcessWithBaseParams(executable, args);
            process.StartInfo.WorkingDirectory = workingDir;

            process.Start();
            return process.StandardOutput.BaseStream;
        }

        public int Invoke(string executable, string arguments, string workingDir, OutputLineDelegate outputLineCallback)
        {
            var process = CreateProcessWithBaseParams(executable, arguments);
            process.StartInfo.WorkingDirectory = workingDir;

            process.Start();            

            var sr = new StreamReader(process.StandardOutput.BaseStream);
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                if (outputLineCallback != null)
                    outputLineCallback(line);                    
            }

            return process.ExitCode;
        }
    }
}
