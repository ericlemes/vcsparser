using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p4codechurn.core.git
{

    public class GitLogParserContext
    {
        public enum State
        {
            ParsingDescription,
            ParsingStats,
            NewCommit
        }

        public GitLogParserContext()
        {
            this.Commits = new List<GitCommit>();
        }

        public GitCommit CurrentCommit { get; set; }

        public List<GitCommit> Commits { get; set; }

        public State CurrentState { get; set; }
    }
}
