using Moq;
using p4codechurn.core;
using p4codechurn.core.git;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace p4codechurn.unittests
{
    public class GivenAChangesetProcessor
    {
        private ChangesetProcessor changesetProcessor;        

        public GivenAChangesetProcessor()
        {
            this.changesetProcessor = new ChangesetProcessor();
        }

        private GitCommit CreateCommitWithAddedLines(string fileName, int addedLines)
        {
            return new GitCommit()
            {
                AuthorDate = new DateTime(2018, 10, 2),
                FileChanges = new List<FileChanges>()
                {
                    new FileChanges()
                    {
                        FileName = fileName,
                        Added = addedLines
                    }
                }
            };
        }

        private GitCommit CreateCommitWithRename(string oldFileName, string newFileName)
        {
            return new GitCommit()
            {
                AuthorDate = new DateTime(2018, 10, 2),
                FileChanges = new List<FileChanges>()
                {
                    new FileChanges()
                    {
                        FileName = newFileName
                    }
                },
                FileRenames = new Dictionary<string, string>()
                {
                    {oldFileName, newFileName }
                }
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
    }

    
}
