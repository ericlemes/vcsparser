using vcsparser.core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace vcsparser.unittests
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

        [Fact]
        public void WhenInvokingWithWorkingDirShouldCallProcessAndRedirectStdOut()
        {
            var processWrapper = new ProcessWrapper();
            var rd = new StreamReader(processWrapper.Invoke("cmd", "/c dir", "C:\\"));
            string output = rd.ReadToEnd();
            Assert.NotEmpty(output);
        }
    }
}
