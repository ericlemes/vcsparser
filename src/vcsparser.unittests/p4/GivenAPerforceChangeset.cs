using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core.p4;
using Xunit;

namespace vcsparser.unittests.p4
{
    public class GivenAPerforceChangeset
    {
        [Fact]
        public void WhenGettingCommitIdentifierShouldReturnChangesetNumber()
        {
            var changeset = new PerforceChangeset();
            changeset.ChangesetNumber = 10;
            Assert.Equal(10, changeset.CommitIdentifier);
        }
    }
}
