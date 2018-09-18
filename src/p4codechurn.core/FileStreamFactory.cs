using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p4codechurn.core
{
    public class FileStreamFactory : IStreamFactory
    {
        public Stream createFileStream(string fileName, FileMode fileMode, FileAccess fileAccess)
        {
            return new FileStream(fileName, fileMode, fileAccess);
        }

        public Stream readFile(string fileName)
        {
            return new FileStream(fileName, FileMode.Open, FileAccess.Read);            
        }
    }
}
