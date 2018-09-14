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
    public class GivenAFileStreamFactory
    {
        [Fact]
        public void WhenCreatingShouldNotReturnNull()
        {
            var fileStreamFactory = new FileStreamFactory();
            Assert.NotNull(fileStreamFactory.createFileStream(this.GetType().Assembly.Location, FileMode.Open, FileAccess.Read));
        }
    }
}
