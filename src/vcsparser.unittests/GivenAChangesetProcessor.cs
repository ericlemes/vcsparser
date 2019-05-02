using Moq;
using vcsparser.core;
using vcsparser.core.git;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Text.RegularExpressions;
using vcsparser.core.bugdatabase;

namespace vcsparser.unittests
{
    public class GivenAChangesetProcessor
    {
        private ChangesetProcessor changesetProcessor;

        private Mock<ILogger> loggerMock;

        public GivenAChangesetProcessor()
        {
            this.loggerMock = new Mock<ILogger>();
            this.changesetProcessor = new ChangesetProcessor("", this.loggerMock.Object);            
        }

        private GitCommit CreateCommitWithAddedLines(string fileName, int addedLines)
        {
            return new GitCommit()
            {
                Author = "defaultcommiter",
                CommiterDate = new DateTime(2018, 10, 2),
                ChangesetFileChanges = new List<FileChanges>()
                {
                    new FileChanges()
                    {
                        FileName = fileName,
                        Added = addedLines
                    }
                },
                CommitHash = "SomeCommitHash"
            };
        }

        private GitCommit CreateCommitWithRename(string oldFileName, string newFileName)
        {
            return new GitCommit()
            {
                Author = "defaultcommiter",
                CommiterDate = new DateTime(2018, 10, 2),
                ChangesetFileChanges = new List<FileChanges>()
                {
                    new FileChanges()
                    {
                        FileName = newFileName
                    }
                },
                ChangesetFileRenames = new Dictionary<string, string>()
                {
                    {oldFileName, newFileName }
                },
                CommitHash = "SomeCommitHash"
            };
        }

        private DailyCodeChurn GetOutputFor(string fileName)
        {
            return this.changesetProcessor.Output[new DateTime(2018, 10, 2)][fileName];
        }

        [Fact]
        public void WhenProcessingSimpleRenameShouldTrackCorrectly()
        {
            //Changesets should be processed in reverse order (as in Git)
            this.changesetProcessor.ProcessChangeset(CreateCommitWithAddedLines("file2", 10));
            this.changesetProcessor.ProcessChangeset(CreateCommitWithRename("file1", "file2"));
            this.changesetProcessor.ProcessChangeset(CreateCommitWithAddedLines("file1", 10));

            Assert.Equal(20, GetOutputFor("file2").Added);
        }

        [Fact]
        public void WhenProcessingSimpleRecursiveRenameShouldTrackCorrectly()
        {
            //Changesets should be processed in reverse order (as in Git)
            this.changesetProcessor.ProcessChangeset(CreateCommitWithAddedLines("file1", 10));
            this.changesetProcessor.ProcessChangeset(CreateCommitWithRename("file2", "file1"));
            this.changesetProcessor.ProcessChangeset(CreateCommitWithAddedLines("file2", 10));
            this.changesetProcessor.ProcessChangeset(CreateCommitWithRename("file1", "file2"));
            this.changesetProcessor.ProcessChangeset(CreateCommitWithAddedLines("file1", 10));

            Assert.Equal(30, GetOutputFor("file1").Added);
        }

        [Fact]
        public void WhenProcessingMultiLevelRecursionShouldTrackCorrectly()
        {
            //Changesets should be processed in reverse order (as in Git)
            this.changesetProcessor.ProcessChangeset(CreateCommitWithAddedLines("file1", 10));
            this.changesetProcessor.ProcessChangeset(CreateCommitWithRename("file3", "file1"));
            this.changesetProcessor.ProcessChangeset(CreateCommitWithAddedLines("file3", 10));
            this.changesetProcessor.ProcessChangeset(CreateCommitWithRename("file2", "file3"));
            this.changesetProcessor.ProcessChangeset(CreateCommitWithAddedLines("file2", 10));
            this.changesetProcessor.ProcessChangeset(CreateCommitWithRename("file1", "file2"));
            this.changesetProcessor.ProcessChangeset(CreateCommitWithAddedLines("file1", 10));

            Assert.Equal(40, GetOutputFor("file1").Added);
        }

        [Fact]
        public void WhenProcessingSameFileRenamedTwiceShouldTrackCorrectly()
        {
            //Changesets should be processed in reverse order (as in Git)
            this.changesetProcessor.ProcessChangeset(CreateCommitWithAddedLines("file3", 10)); 
            this.changesetProcessor.ProcessChangeset(CreateCommitWithRename("file1", "file3"));
            this.changesetProcessor.ProcessChangeset(CreateCommitWithAddedLines("file1", 10)); //New history for file1
            this.changesetProcessor.ProcessChangeset(CreateCommitWithAddedLines("file2", 10));
            this.changesetProcessor.ProcessChangeset(CreateCommitWithRename("file1", "file2"));
            this.changesetProcessor.ProcessChangeset(CreateCommitWithAddedLines("file1", 10));

            Assert.Equal(20, GetOutputFor("file2").Added);
            Assert.Equal(20, GetOutputFor("file3").Added);
        }

        [Fact]
        public void WhenSameFileNameRenamedTwiceShouldTrackCorrectly()
        {            
            this.changesetProcessor.ProcessChangeset(CreateCommitWithAddedLines("file2", 10));
            this.changesetProcessor.ProcessChangeset(CreateCommitWithRename("file1", "file2"));
            this.changesetProcessor.ProcessChangeset(CreateCommitWithAddedLines("file1", 10));
            this.changesetProcessor.ProcessChangeset(CreateCommitWithRename("file2", "file1"));
            this.changesetProcessor.ProcessChangeset(CreateCommitWithAddedLines("file2", 10));
            this.changesetProcessor.ProcessChangeset(CreateCommitWithRename("file1", "file2"));
            this.changesetProcessor.ProcessChangeset(CreateCommitWithAddedLines("file1", 10));

            Assert.Equal(40, GetOutputFor("file2").Added);
        }

        [Fact]
        public void WhenProcessingChangesetAndHasBugRegexesShouldEvaluateBugRegexes()
        {            
            this.changesetProcessor = new ChangesetProcessor(@"gramolias+;bug+", this.loggerMock.Object);
            var c = CreateCommitWithAddedLines("file2", 10);
            c.ChangesetFileChanges[0].Deleted = 5;
            c.ChangesetMessage= "This is a comment a newline \n\r and a bug";
            this.changesetProcessor.ProcessChangeset(c);
            Assert.Equal(1, GetOutputFor("file2").NumberOfChangesWithFixes);
            Assert.Equal(15, GetOutputFor("file2").TotalLinesChangedWithFixes);
        }

        [Fact]
        public void WhenProcessingChangesetAndHasBugRegexesThatDoesnotMatchShouldNotCountAsBug()
        {
            this.changesetProcessor = new ChangesetProcessor(@"gramolias+;bug+", this.loggerMock.Object);
            var c = CreateCommitWithAddedLines("file2", 10);
            c.ChangesetFileChanges[0].Deleted = 5;
            c.ChangesetMessage = "This is a comment a newline new feature";
            this.changesetProcessor.ProcessChangeset(c);
            Assert.Equal(0, GetOutputFor("file2").NumberOfChangesWithFixes);
            Assert.Equal(0, GetOutputFor("file2").TotalLinesChangedWithFixes);
        }

        [Fact]
        public void WhenProcessingChangesetShouldAppendAuthor()
        {
            var c = CreateCommitWithAddedLines("file1", 10);
            c.Author = "author1";
            this.changesetProcessor.ProcessChangeset(c);
            Assert.Equal("author1", GetOutputFor("file1").Authors[0].Author);
            Assert.Equal(1, GetOutputFor("file1").Authors[0].NumberOfChanges);
            Assert.Single(GetOutputFor("file1").Authors);
        }

        [Fact]
        public void WhenProcessingMultipleChangesetsShouldAppendAuthors()
        {
            var c = CreateCommitWithAddedLines("file1", 10);
            c.Author = "author1";
            this.changesetProcessor.ProcessChangeset(c);

            c = CreateCommitWithAddedLines("file1", 10);
            c.Author = "author2";
            this.changesetProcessor.ProcessChangeset(c);

            //It will return in ascending order, ignoring added order.
            Assert.Equal("author1", GetOutputFor("file1").Authors[0].Author);
            Assert.Equal(1, GetOutputFor("file1").Authors[0].NumberOfChanges);
            Assert.Equal("author2", GetOutputFor("file1").Authors[1].Author);
            Assert.Equal(1, GetOutputFor("file1").Authors[1].NumberOfChanges);
            Assert.Equal(2, GetOutputFor("file1").Authors.Count);
        }

        [Fact]
        public void WhenProcessingMultipleChangesetsShouldHaveDistinctAuthors()
        {
            var c = CreateCommitWithAddedLines("file1", 10);
            c.Author = "author1";
            this.changesetProcessor.ProcessChangeset(c);

            c = CreateCommitWithAddedLines("file1", 10);
            c.Author = "AUTHOR1"; //case-insensitive
            this.changesetProcessor.ProcessChangeset(c);

            //It will return in ascending order, ignoring added order.
            Assert.Equal("author1", GetOutputFor("file1").Authors[0].Author);
            Assert.Equal(2, GetOutputFor("file1").Authors[0].NumberOfChanges);
            Assert.Single(GetOutputFor("file1").Authors);
        }

        [Fact]
        public void WhenProcessingBugDatabaseChangesetAndHasBugRegexesShouldEvaluateBugRegexes()
        {
            this.changesetProcessor = new ChangesetProcessor(@"gramolias+;bug+", this.loggerMock.Object);
            this.changesetProcessor.WorkItemCache.Add("SomeCommitHash", new WorkItem());

            var c = CreateCommitWithAddedLines("file2", 10);
            c.ChangesetFileChanges[0].Deleted = 5;
            c.ChangesetMessage = "This is a comment a newline \n\r and a bug";
            this.changesetProcessor.ProcessChangeset(c);

            Assert.Equal(1, GetOutputFor("file2").BugDatabse.NumberOfChangesWithFixes);
            Assert.Equal(15, GetOutputFor("file2").BugDatabse.TotalLinesChanged);
        }

        [Fact]
        public void WhenProcessingBugDatabaseChangesetAndHasBugRegexesThatDoesnotMatchShouldNotCountAsBug()
        {
            this.changesetProcessor = new ChangesetProcessor(@"gramolias+;bug+", this.loggerMock.Object);
            this.changesetProcessor.WorkItemCache.Add("SomeCommitHash", new WorkItem());

            var c = CreateCommitWithAddedLines("file2", 10);
            c.ChangesetFileChanges[0].Deleted = 5;
            c.ChangesetMessage = "This is a comment a newline new feature";
            this.changesetProcessor.ProcessChangeset(c);

            Assert.Equal(0, GetOutputFor("file2").BugDatabse.NumberOfChangesWithFixes);
            Assert.Equal(15, GetOutputFor("file2").BugDatabse.TotalLinesChanged);
        }

        [Fact]
        public void WhenProcessingBugDatabaseChangesetAndDailyCodeChurnContainsBugDatabaseThenAppenedExisting()
        {
            this.changesetProcessor = new ChangesetProcessor(@"gramolias+;bug+", this.loggerMock.Object);
            this.changesetProcessor.WorkItemCache.Add("SomeCommitHash", new WorkItem());

            var c1 = CreateCommitWithAddedLines("file2", 10);
            c1.ChangesetFileChanges[0].Deleted = 5;
            c1.ChangesetMessage = "This is a comment a newline \n\r and a bug";
            this.changesetProcessor.ProcessChangeset(c1);

            var c2 = CreateCommitWithAddedLines("file2", 10);
            c2.ChangesetFileChanges[0].Deleted = 5;
            c2.ChangesetMessage = "This is a comment a newline \n\r and a bug";
            this.changesetProcessor.ProcessChangeset(c2);

            Assert.Equal(2, GetOutputFor("file2").BugDatabse.NumberOfChangesWithFixes);
            Assert.Equal(30, GetOutputFor("file2").BugDatabse.TotalLinesChanged);
        }
    }
}
