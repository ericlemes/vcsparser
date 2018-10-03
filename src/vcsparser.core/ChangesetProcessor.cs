using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public void ProcessChangeset(IChangeset changeset)
        {
            if (changeset == null)
                return;

            UpdateRenameCache(changeset);

            if (!dict.ContainsKey(changeset.Timestamp.Date))
                dict.Add(changeset.Timestamp.Date, new Dictionary<string, DailyCodeChurn>());

            foreach (var c in changeset.FileChanges)
            {
                var fileName = GetFileNameConsideringRenames(c.FileName);

                if (!dict[changeset.Timestamp.Date].ContainsKey(fileName))
                    dict[changeset.Timestamp.Date].Add(fileName, new DailyCodeChurn());

                var dailyCodeChurn = dict[changeset.Timestamp.Date][fileName];
                dailyCodeChurn.Timestamp = changeset.Timestamp.Date.ToString(DailyCodeChurn.DATE_FORMAT);
                dailyCodeChurn.FileName = fileName;
                dailyCodeChurn.Added += c.Added;
                dailyCodeChurn.Deleted += c.Deleted;
                dailyCodeChurn.ChangesBefore += c.ChangedBefore;
                dailyCodeChurn.ChangesAfter += c.ChangedAfter;
                dailyCodeChurn.NumberOfChanges += 1;
            }
        }

        private void UpdateRenameCache(IChangeset changeset)
        {            
            foreach (var pair in changeset.FileRenames) {
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
