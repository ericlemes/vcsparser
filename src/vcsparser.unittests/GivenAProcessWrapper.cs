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
            var invoke = processWrapper.Invoke("cmd", "/c dir");
            Assert.NotEmpty(invoke.Item2);            
        }

        [Fact]
        public void WhenInvokingAndCallbackShouldCallProcessAndRedirectStdOut()
        {
            string data = "";
            var processWrapper = new ProcessWrapper();
            var invoke = processWrapper.Invoke("cmd", "/c dir", (l) => { data += l; });
            Assert.NotEmpty(data);
        }

        [Fact]
        public void WhenInvokingWithWorkingDirShouldCallProcessAndRedirectStdOut()
        {
            var processWrapper = new ProcessWrapper();
            var invoke = processWrapper.Invoke("cmd", "/c dir", "C:\\");
            Assert.NotEmpty(invoke.Item2);
        }

        [Fact]
        public void WhenInvokingWithWorkingDirAndCallbackShouldInvokeCallbackAndReturnZero()
        {
            int lineCount = 0;
            var processWrapper = new ProcessWrapper();
            Assert.Equal(0, processWrapper.Invoke("cmd", "/c dir", "C:\\", (l) => { lineCount++; }));
            Assert.True(lineCount > 0);
        }

        [Fact]
        public void WhenInvokingWithWorkingDirAndReturnZero()
        {
            var processWrapper = new ProcessWrapper();
            var invoke = processWrapper.Invoke("cmd", "/c dir", "C:\\");
            Assert.Equal(0, invoke.Item1);
            Assert.True(invoke.Item2.Count > 0);
        }

        [Fact]
        public void WhenInvokingWithWorkingDirAndCallbackShouldReturn0()
        {            
            var processWrapper = new ProcessWrapper();
            Assert.Equal(0, processWrapper.Invoke("cmd", "/c dir", "C:\\", null));            
        }

        [Fact]
        public void WhenInvokingWithWorkingDirShouldReturn0()
        {
            var processWrapper = new ProcessWrapper();
            Assert.Equal(0, processWrapper.Invoke("cmd", "/c dir", "C:\\").Item1);
        }
    }
}
