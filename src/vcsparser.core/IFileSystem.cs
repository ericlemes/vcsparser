using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core
{
    public interface IFileSystem
    {
        IEnumerable<IFile> GetFiles(string directory, string mask);

        string GetFullPath(string directory);
        string GetParentFullName(string directory);
    }
}
