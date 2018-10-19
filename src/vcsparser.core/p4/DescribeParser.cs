using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core.p4
{
    public class DescribeParser : IDescribeParser
    {
        public PerforceChangeset Parse(Stream ms)
        {
            var result = new PerforceChangeset();
            FileChanges currentFileChanges = null;

            var sr = new StreamReader(ms);
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                ParseLine(ref currentFileChanges, line, result);
            }

            return result;
        }

        private void ParseLine(ref FileChanges currentFileChanges, string line, PerforceChangeset changeset)
        {            
            if (line.StartsWith("Change "))
                ParseHeader(line, changeset);
            else if (line.StartsWith("\t")) 
                changeset.AppendMessage(line.Substring(1));
            else if (line.StartsWith("===="))
                ParseNewFileChanges(ref currentFileChanges, line, changeset);
            else if (line.StartsWith("add "))
                ParseAddedLines(ref currentFileChanges, line, changeset);
            else if (line.StartsWith("deleted "))
                ParseDeletedLines(ref currentFileChanges, line, changeset);
            else if (line.StartsWith("changed "))
                ParseChangedLines(ref currentFileChanges, line, changeset);
        }

        private void ParseChangedLines(ref FileChanges currentFileChanges, string line, PerforceChangeset changeset)
        {
            var splitted = line.Split(' ');
            currentFileChanges.ChangedBefore = Convert.ToInt32(splitted[3]);
            currentFileChanges.ChangedAfter = Convert.ToInt32(splitted[5]);
        }

        private void ParseDeletedLines(ref FileChanges currentFileChanges, string line, PerforceChangeset changeset)
        {
            var splitted = line.Split(' ');
            currentFileChanges.Deleted = Convert.ToInt32(splitted[3]);
        }

        private void ParseAddedLines(ref FileChanges currentFileChanges, string line, PerforceChangeset changeset)
        {
            var splitted = line.Split(' ');
            currentFileChanges.Added = Convert.ToInt32(splitted[3]);
        }

        private void ParseNewFileChanges(ref FileChanges currentFileChanges, string line, PerforceChangeset changeset)
        {
            currentFileChanges = new FileChanges();
            changeset.FileChanges.Add(currentFileChanges);

            currentFileChanges.FileName = line.Split('#')[0].Split(' ')[1];
        }

        private void ParseHeader(string line, PerforceChangeset changeset)
        {            
            var sliced = line.Split(' ');
            if (sliced.Length < 7)
                throw new InvalidFormatException("Invalid header, line starting with Change ");

            changeset.ChangesetNumber = Convert.ToInt32(sliced[1]);
            changeset.Author = sliced[3];
            var slicedAuthor = changeset.Author.Split('@');
            if (slicedAuthor.Length < 2)
                throw new InvalidFormatException("Invalid author. Expected author@workspace");
            changeset.AuthorName = slicedAuthor[0];
            changeset.AuthorWorkspace = slicedAuthor[1];

            try
            {
                changeset.Timestamp = ParseTimestamp(sliced[5], sliced[6]);
            }
            catch
            {
                throw new InvalidFormatException("Invalid date and time. Expecting YYYY/MM/DD HH:MM:SS");
            }
        }

        private DateTime ParseTimestamp(string date, string time)
        {
            var dateSliced = date.Split('/');
            var timeSliced = time.Split(':');

            return new DateTime(Convert.ToInt32(dateSliced[0]), Convert.ToInt32(dateSliced[1]), Convert.ToInt32(dateSliced[2]),
                Convert.ToInt32(timeSliced[0]), Convert.ToInt32(timeSliced[1]), Convert.ToInt32(timeSliced[2]));
        }
    }
}
