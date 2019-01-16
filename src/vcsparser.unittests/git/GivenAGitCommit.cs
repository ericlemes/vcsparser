using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core.git;
using Xunit;

namespace vcsparser.unittests
{
    public class GivenAGitCommit
    {
        [Fact]
        public void WhenGettingCommitIdentifierShouldReturnCommitHash()
        {
            var c = new GitCommit();
            c.CommitHash = "a";
            Assert.Equal("a", c.ChangesetIdentifier);
        }
    }
}
