using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.PowerShell
{
    public interface ICmdlet
    {
        void WriteObject(object obj);
    }
}
