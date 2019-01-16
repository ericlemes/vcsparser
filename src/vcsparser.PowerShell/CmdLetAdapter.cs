using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.PowerShell
{
    public class CmdletAdapter : ICmdlet
    {
        private Cmdlet cmdlet;

        public CmdletAdapter(Cmdlet cmdlet)
        {
            this.cmdlet = cmdlet;
        }

        public void WriteObject(object obj)
        {
            this.cmdlet.WriteObject(obj);
        }
    }
}
