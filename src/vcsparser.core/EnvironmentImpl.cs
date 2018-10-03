using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core
{
    public class EnvironmentImpl : IEnvironment
    {
        public DateTime GetCurrentDateTime()
        {
            return DateTime.Now;
        }
    }
}
