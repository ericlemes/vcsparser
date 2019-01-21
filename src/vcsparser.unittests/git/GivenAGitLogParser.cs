using vcsparser.core.git;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace vcsparser.unittests.git
{
    public class GivenAGitLogParser
    {
        private GitLogParser parser;

        public GivenAGitLogParser()
        {
            parser = new GitLogParser();
        }

        private MemoryStream GetStreamWithContent(string content)
        {
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);
            sw.Write(content);
            sw.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        [Fact]
        public void WhenConvertingIsoDateStringToDateShouldReturnExpectedValue()
        {            
            var dateTime = parser.Iso8601StringToDateTime("2018-09-19T14:19:14+01:00");
            Assert.Equal(new DateTime(2018, 09, 19, 14, 19, 14), dateTime);
        }

        [Fact]
        public void WhenParsingShouldReturnExpectedResult()
        {
            var stream = GetStreamWithContent(Resources.GitExample1);
            var changesets = parser.Parse(stream);

            Assert.Equal(2, changesets.Count);
        }

        [Fact]
        public void WhenParsingCommitWithDescriptionShouldReturnExpectedValues() {
            var stream = GetStreamWithContent(Resources.GitExample1);            

            GitCommit commit = this.parser.Parse(stream)[0];
            Assert.Equal("82419fcdc1fca1f8b14905366159837bfe8a1be4", commit.CommitHash);
            Assert.Equal("Author Name <author@email.com>", commit.Author);
            Assert.Equal(new DateTime(2018, 09, 19, 14, 19, 14), commit.AuthorDate);
            Assert.Equal("Author Name <author@email.com>", commit.Commiter);
            Assert.Equal(new DateTime(2018, 09, 19, 14, 19, 14), commit.CommiterDate);
            Assert.Equal("Commit message 1\r\n\r\n* Long description 1 line 1\r\n* Long description 1 line 2\r\n", commit.ChangesetMessage);
        }

        [Fact]
        public void WhenParsingCommitWithoutDescriptionShouldReturnExpectedValues(){
            var stream = GetStreamWithContent(Resources.GitExample1);

            GitCommit commit = this.parser.Parse(stream)[1];            
            Assert.Equal("31b45b8417418c3562d19eab8830ed786ac40f40", commit.CommitHash);
            Assert.Equal("Author Name <author@email.com>", commit.Author);
            Assert.Equal(new DateTime(2018, 09, 18, 16, 48, 22), commit.AuthorDate);
            Assert.Equal("Author Name <author@email.com>", commit.Commiter);
            Assert.Equal(new DateTime(2018, 09, 18, 16, 48, 22), commit.CommiterDate);
            Assert.Equal("This one only has the commit message. No long description\r\n", commit.ChangesetMessage);
        }
        
        [Fact]
        public void WhenParsingCommitShouldParseStatsCorrectly(){
            var stream = GetStreamWithContent(Resources.GitExample1);

            GitCommit commit = this.parser.Parse(stream)[0];
            Assert.Equal(3, commit.ChangesetFileChanges.Count);
            Assert.Equal(1, commit.ChangesetFileChanges[0].Added);
            Assert.Equal(1, commit.ChangesetFileChanges[0].Deleted);
            Assert.Equal("src/dir1/File1.cs", commit.ChangesetFileChanges[0].FileName);

            Assert.Equal(10, commit.ChangesetFileChanges[1].Added);
            Assert.Equal(1, commit.ChangesetFileChanges[1].Deleted);
            Assert.Equal("src/dir1/File2.cs", commit.ChangesetFileChanges[1].FileName);

            Assert.Equal(1, commit.ChangesetFileChanges[2].Added);
            Assert.Equal(10, commit.ChangesetFileChanges[2].Deleted);
            Assert.Equal("src/dir1/File3.cs", commit.ChangesetFileChanges[2].FileName);
        }

        [Fact]
        public void WhenParsingSubsequentCommitsShouldParseStatsCorrectly() {
            var stream = GetStreamWithContent(Resources.GitExample1);

            GitCommit commit = this.parser.Parse(stream)[1];
            Assert.Equal(2, commit.ChangesetFileChanges.Count);

            Assert.Equal(4, commit.ChangesetFileChanges[1].Added);
            Assert.Equal(3, commit.ChangesetFileChanges[1].Deleted);
            Assert.Equal("src/dir2/SomeOtherFile.cs", commit.ChangesetFileChanges[1].FileName);
        }

        [Fact]
        public void WhenParsingCommitWithBinaryFilesShouldGetResultsCorrectly()
        {
            var stream = GetStreamWithContent(Resources.GitExample2);

            GitCommit commit = this.parser.Parse(stream)[0];
            Assert.Single(commit.ChangesetFileChanges);
            Assert.Equal("src/dir-with-dashes/File1.cs", commit.ChangesetFileChanges[0].FileName);
            Assert.Equal(0, commit.ChangesetFileChanges[0].Added);
            Assert.Equal(0, commit.ChangesetFileChanges[0].Deleted);
        }

        [Fact]
        public void WhenParsingLogWithRenameHistoryShouldReturnExpectedValues()
        {
            var stream = GetStreamWithContent(Resources.GitExample3);
            var commits = this.parser.Parse(stream);

            Assert.Equal(3, commits.Count);
            Assert.Equal(3, commits[0].ChangesetFileChanges[0].Added);
            Assert.Equal("test1.txt", commits[0].ChangesetFileChanges[0].FileName);

            Assert.Equal("test2.txt", commits[1].ChangesetFileChanges[0].FileName);
            Assert.Single(commits[1].ChangesetFileRenames);
            Assert.Equal("test1.txt", commits[1].ChangesetFileRenames["test2.txt"]);
        }

        [Fact]
        public void WhenParsingFileNameRenameWithPartialTreeShouldReturnExpectedValues()
        {
            var context = new GitLogParserContext();
            context.CurrentCommit = new GitCommit();
            parser.ProcessRenames(context, "some/common/path/root/{OldFileName.h => NewFileName.h}");

            Assert.Equal("some/common/path/root/OldFileName.h", context.CurrentCommit.ChangesetFileRenames["some/common/path/root/NewFileName.h"]);
        }

        [Fact]
        public void WhenParsingFileNameRenameWithoutPartialTreeShouldReturnExpectedValues()
        {
            var context = new GitLogParserContext();
            context.CurrentCommit = new GitCommit();
            parser.ProcessRenames(context, "OldFileName.h => NewFileName.h");

            Assert.Equal("OldFileName.h", context.CurrentCommit.ChangesetFileRenames["NewFileName.h"]);
        }

        [Fact]
        public void WhenParsingDirectoryRenameShouldReturnExpectedValues()
        {
            var context = new GitLogParserContext();
            context.CurrentCommit = new GitCommit();
            parser.ProcessRenames(context, "some/root/{ => another/added/dir}/SomeFile.cpp");

            Assert.Equal("some/root/SomeFile.cpp", context.CurrentCommit.ChangesetFileRenames["some/root/another/added/dir/SomeFile.cpp"]);
        }

        [Fact]
        public void WhenParsingMergeShouldNotThrow()
        {
            var stream = GetStreamWithContent(Resources.GitExample4);

            this.parser.Parse(stream);
        }

        
}
}
