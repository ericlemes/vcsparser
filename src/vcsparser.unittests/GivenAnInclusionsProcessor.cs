using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core;
using Xunit;

namespace vcsparser.unittests
{
    public class GivenAnInclusionsProcessor
    {
        private InclusionsProcessor processor;

        public GivenAnInclusionsProcessor()
        {
            this.processor = new InclusionsProcessor("**/*.cpp,**/*.c,**/*.h,**/*.hpp");
        }

        [Fact]
        public void WhenValidExamplesThenShouldReturnTrue()
        {
            Assert.True(this.processor.IsIncluded("somefile.cpp"));
            Assert.True(this.processor.IsIncluded("somedir/somefile.cpp"));
            Assert.True(this.processor.IsIncluded("somedir/otherdir/somefile.cpp"));
            Assert.True(this.processor.IsIncluded("somefil?.cpp"));
            Assert.True(this.processor.IsIncluded("somefile.CPP"));
            Assert.True(this.processor.IsIncluded("somefile.c"));
            Assert.True(this.processor.IsIncluded("somefile.hPp"));
            Assert.True(this.processor.IsIncluded("somedir/somefile.hPp"));
        }

        [Fact]
        public void WhenInvalidExamplesThenShouldNotMatch()
        {
            Assert.False(this.processor.IsIncluded("noextension"));
            Assert.False(this.processor.IsIncluded("noextension.otherextension"));
            Assert.False(this.processor.IsIncluded("otherdirectory.cpp/noextension"));
        }

        [Fact]
        public void WhenCreatingWithEmptyExpressionsThenShouldNotThrow()
        {
            new InclusionsProcessor(null);
            new InclusionsProcessor("");
        }

    }
}
