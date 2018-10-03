using p4codechurn.core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace p4codechurn.unittests
{
    public class GivenAProcessWrapper
    {
        [Fact]
        public void WhenInvokingShouldCallProcessAndRedirectStdOut()
        {
            var processWrapper = new ProcessWrapper();
            var rd = new StreamReader(processWrapper.Invoke("cmd", "/c dir"));
            string output = rd.ReadToEnd();
            Assert.NotEmpty(output);            
        }
    }
}
