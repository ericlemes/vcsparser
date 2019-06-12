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

        private void StartProcess(Process process, OutputLineDelegate outputLineCallback)
        {
            process.Start();

            var sr = new StreamReader(process.StandardOutput.BaseStream);
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                if (outputLineCallback != null)
                    outputLineCallback(line);
            }
        }

        public int Invoke(string executable, string arguments, OutputLineDelegate outputLineCallback)
        {
            var process = CreateProcessWithBaseParams(executable, arguments);
            StartProcess(process, outputLineCallback);
            return process.ExitCode;
        }

        public int Invoke(string executable, string arguments, string workingDir, OutputLineDelegate outputLineCallback)
        {
            var process = CreateProcessWithBaseParams(executable, arguments);
            process.StartInfo.WorkingDirectory = workingDir;
            StartProcess(process, outputLineCallback);
            return process.ExitCode;
        }
    }
}
