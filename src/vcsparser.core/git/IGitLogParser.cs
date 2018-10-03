using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core.git
{
    public interface IGitLogParser
    {
        List<GitCommit> Parse(Stream stream);
    }
}
