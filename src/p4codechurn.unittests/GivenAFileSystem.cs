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
    public class GivenAFileSystem
    {
        private FileSystem fileSystem;

        public GivenAFileSystem()
        {
            this.fileSystem = new FileSystem();
        }

        [Fact]
        public void WhenGettingFilesShouldReturnExpectedFiles()
        {
            var files = this.fileSystem.GetFiles(new FileInfo(this.GetType().Assembly.Location).Directory.FullName, "p4codechurn.unittests.dll");
            Assert.Single(files);
            Assert.Equal(this.GetType().Assembly.Location.ToLower(), files.First().FileName.ToLower());
        }
    }
}
