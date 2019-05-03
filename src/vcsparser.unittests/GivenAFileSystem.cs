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
            var files = this.fileSystem.GetFiles(new FileInfo(this.GetType().Assembly.Location).Directory.FullName, "vcsparser.unittests.dll");
            Assert.Single(files);
            Assert.Equal(this.GetType().Assembly.Location.ToLower(), files.First().FileName.ToLower());
        }

        [Fact]
        public void WhenGettingFullNameShouldReturnExpectedFullName()
        {
            var fullName = this.fileSystem.GetFullName(this.GetType().Assembly.Location);
            Assert.Equal(new FileInfo(this.GetType().Assembly.Location).FullName, fullName);
        }
    }
}
