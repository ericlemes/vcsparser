using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core
{
    public delegate void OutputLineDelegate(string line);

    public interface IProcessWrapper
    {
        Stream Invoke(string executable, string arguments);
        Stream Invoke(string executable, string arguments, string workingDir);

        /// <summary>
        /// Calls an external executable with specified arguments, invoking a callback for each line of standard
        /// output.
        /// </summary>
        /// <param name="executable"></param>
        /// <param name="arguments"></param>
        /// <param name="workingDir"></param>
        /// <param name="outputLineCallback"></param>
        /// <returns></returns>
        int Invoke(string executable, string arguments, string workingDir, OutputLineDelegate outputLineCallback);
    }
}
