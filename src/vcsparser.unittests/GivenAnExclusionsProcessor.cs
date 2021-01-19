using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core;
using Xunit;

namespace vcsparser.unittests
{
    public class GivenAnExclusionsProcessor
    {
        private ExclusionsProcessor processor;

        public GivenAnExclusionsProcessor()
        {
            this.processor = new ExclusionsProcessor("external/**,Tools/Libraries/**,**/*.xml,**/SomeSpecificFile.h,SomeDir/OtherDir/SomeFile.*,**/*.tests.cpp,**/Mock*.*,**/*Tests.cpp,dir?/file.txt");
        }        

        [Fact]
        public void WhenHaveNoExclusionsAndCheckIsExcludedThenShouldReturnFalse()
        {
            this.processor = new ExclusionsProcessor("");
            Assert.False(this.processor.IsExcluded("SomeFile.cs"));
            Assert.False(this.processor.IsExcluded("SomeDir/SomeFile.cs"));
            Assert.False(this.processor.IsExcluded("somedir.xml/SomeOtherCompletelyDifferentFile.cpp"));
        }

        [Fact]
        public void WhenHaveExclusionsAndCheckIsExcludedForMatchingPatternShouldReturnTrue()
        {            
            Assert.True(this.processor.IsExcluded("external/SomeFile.DoesNotMatter"));
            Assert.True(this.processor.IsExcluded("external/SomeFileOtherDir/DoesNotMatter.cpp"));
            Assert.True(this.processor.IsExcluded("Tools/Libraries/DoesNotMatter.cpp"));
            Assert.True(this.processor.IsExcluded("Tools/Libraries/OtherDir/DoesNotMatter"));
            Assert.True(this.processor.IsExcluded("something.xml"));
            Assert.True(this.processor.IsExcluded("SomeDir/something.xml"));
            Assert.True(this.processor.IsExcluded("SomeDir/SomeOtherDir2/SomeSpecificFile.h"));
            Assert.True(this.processor.IsExcluded("SomeDir/OtherDir/SomeFile.x"));
            Assert.True(this.processor.IsExcluded("SomeFile.tests.cpp"));
            Assert.True(this.processor.IsExcluded("SomeDir/SomeFile.tests.cpp"));
            Assert.True(this.processor.IsExcluded("MockSomething.txt"));
            Assert.True(this.processor.IsExcluded("SomeDir/MockOtherThing.cs"));
            Assert.True(this.processor.IsExcluded("SomeThingThatEndsOnTests.cpp"));
            Assert.True(this.processor.IsExcluded("dir1/file.txt"));            
        }

        [Fact]
        public void WhenHaveExclusionsWithSingleCharThenShouldNotMatchDirectorySeparator()
        {
            Assert.False(this.processor.IsExcluded("dir/file.txt"));            
        }
    }
}
