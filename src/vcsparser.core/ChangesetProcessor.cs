using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace vcsparser.core
{
    public class ChangesetProcessor
    {
        private Dictionary<DateTime, Dictionary<string, DailyCodeChurn>> dict = new Dictionary<DateTime, Dictionary<string, DailyCodeChurn>>();
        public Dictionary<DateTime, Dictionary<string, DailyCodeChurn>> Output
        {
            get { return dict; }
        }

        private Dictionary<string, string> renameCache = new Dictionary<string, string>();

        private List<Regex> bugRegexes;        

        private ILogger logger;

        public int ChangesetsWithBugs
        {
            get; private set;
        }

        public ChangesetProcessor(string bugRegexes, ILogger logger)
        {
            this.bugRegexes = new List<Regex>();
            if (bugRegexes != null)
            {
                foreach (var r in bugRegexes.Split(';'))
                    this.bugRegexes.Add(new Regex(r));
            }            
            this.logger = logger;
        }

        public void ProcessChangeset(IChangeset changeset)
        {
            if (changeset == null)
                return;

            UpdateRenameCache(changeset);

            if (!dict.ContainsKey(changeset.ChangesetTimestamp.Date))
                dict.Add(changeset.ChangesetTimestamp.Date, new Dictionary<string, DailyCodeChurn>());

            bool containsBugs = CheckAndIncrementIfChangesetContainsBug(changeset);            

            foreach (var c in changeset.ChangesetFileChanges)
            {
                var fileName = GetFileNameConsideringRenames(c.FileName);

                if (!dict[changeset.ChangesetTimestamp.Date].ContainsKey(fileName))
                    dict[changeset.ChangesetTimestamp.Date].Add(fileName, new DailyCodeChurn());

                var dailyCodeChurn = dict[changeset.ChangesetTimestamp.Date][fileName];
                dailyCodeChurn.Timestamp = changeset.ChangesetTimestamp.Date.ToString(DailyCodeChurn.DATE_FORMAT);
                dailyCodeChurn.FileName = fileName;
                dailyCodeChurn.Added += c.Added;
                dailyCodeChurn.Deleted += c.Deleted;
                dailyCodeChurn.ChangesBefore += c.ChangedBefore;
                dailyCodeChurn.ChangesAfter += c.ChangedAfter;
                dailyCodeChurn.NumberOfChanges += 1;
                if (containsBugs)
                {
                    dailyCodeChurn.NumberOfChangesWithFixes++;
                    dailyCodeChurn.AddedWithFixes += c.Added;
                    dailyCodeChurn.DeletedWithFixes += c.Deleted;
                    dailyCodeChurn.ChangesBeforeWithFixes += c.ChangedBefore;
                    dailyCodeChurn.ChangesAfterWithFixes += c.ChangedAfter;
                }
            }            
        }

        private bool CheckAndIncrementIfChangesetContainsBug(IChangeset changeset)
        {
            if (String.IsNullOrEmpty(changeset.ChangesetMessage)) 
                return false;

            foreach (var regex in this.bugRegexes)
                if (regex.IsMatch(changeset.ChangesetMessage))
                {
                    ChangesetsWithBugs++;
                    return true;
                }
            return false;
        }

        private void UpdateRenameCache(IChangeset changeset)
        {            
            foreach (var pair in changeset.ChangesetFileRenames) {
                string value = GetDestinationFileFollowingRenames(pair.Value);

                if (!renameCache.ContainsKey(pair.Key)) 
                    renameCache.Add(pair.Key, value);                
                else                
                    renameCache[pair.Key] = value;                
            }
        }

        private string GetDestinationFileFollowingRenames(string fileName, string finalFileName = null)
        {
            if (finalFileName == fileName)
                return fileName;

            if (renameCache.ContainsKey(fileName))
                return GetDestinationFileFollowingRenames(renameCache[fileName], fileName);
            else
                return fileName;
                        
        }

        private string GetFileNameConsideringRenames(string fileName)
        {
            if (renameCache.ContainsKey(fileName))            
                return renameCache[fileName];            

            return fileName;
        }
    }
}
