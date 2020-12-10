using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace vcsparser.unittests
{
    public class GivenARegex
    {
        [Fact]
        public void WhenHasWordBugShouldMatch()
        {            
            var regex = new Regex(@"(?i)((?:^|\W)bug(?:$|\W)|(?:^|\W)crash(?:$|\W)|(?:^|\W)crashes(?:$|\W))|(?:^|\W)bugs(?:$|\W)|(?:^|\W)fix(?:$|\W)|(?:^|\W)bugfix(?:$|\W)|(?:^|\W)bugfixes(?:$|\W)|(?:^|\W)\\\\root\\release_\d+(?:$|\W)|(?:^|\W)//Gravity/release_\d+(?:$|\W)");
            Assert.Matches(regex, "This is a changeset with the word bug .");
            Assert.Matches(regex, "This is a changeset with BUG");
            Assert.Matches(regex, "This is a changeset with bUg");
            Assert.Matches(regex, "This is a changeset with bUgs");
            Assert.Matches(regex, "This is a changeset with word crash");
            Assert.Matches(regex, "This is a changeset with word crashes");
            Assert.Matches(regex, "This is a changeset with word bugfix");
            Assert.Matches(regex, "This is a changeset with word fix");
            Assert.Matches(regex, "This is a changeset with word bugfixes");            
            Assert.Matches(regex, "This is a changeset with word Bug[1324]");
            Assert.Matches(regex, "This is a changeset with word [Bug 1324]");
            Assert.Matches(regex, "This is a changeset with word Bug:1324");
            Assert.Matches(regex, "\r\n[Bug: 15339]\r\n");
            Assert.Matches(regex, @"This is a changeset with merge from \\root\release_021 ");
            Assert.Matches(regex, @"Merging //Gravity/release_016 to mainline_minimal (//Gravity/mainline_minimal)");

            Assert.DoesNotMatch(regex, "Any other changeset.");
            Assert.DoesNotMatch(regex, "Changeset with partial word like somebugword");
            Assert.DoesNotMatch(regex, "Changeset with partial word like somecrashword");
        }

        [Fact]
        public void WhenHasBugWithNumberInBracketsShouldMatch()
        {
            var regex = new Regex(@"(?i)(\[\s*bug\s+\#\d+\s*\])");
            Assert.Matches(regex, "[BUG #1234]");
            Assert.Matches(regex, "[bug #1234]");
            Assert.Matches(regex, "[bug  #1234]");
            Assert.Matches(regex, " [bug  #1234]");
            Assert.Matches(regex, "[bug  #1]asdf");
            Assert.Matches(regex, "[bug  #1] ");
            Assert.Matches(regex, "asdf[bug  #1]asdf");
            Assert.Matches(regex, "[  bug  #1] ");
            Assert.Matches(regex, "[bug  #1  ] ");
            Assert.DoesNotMatch(regex, "bug");
        }

    }
}
