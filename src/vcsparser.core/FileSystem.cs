using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core
{
    public class FileSystem : IFileSystem
    {
        public IEnumerable<IFile> GetFiles(string directory, string mask)
        {
            var dir = new DirectoryInfo(directory);
            return dir.GetFiles(mask).Select(fi => new FileImpl(fi.FullName));            
        }

        public string GetFullName(string directory)
        {
            var dir = new DirectoryInfo(directory);
            return dir.FullName;
        }
    }
}
