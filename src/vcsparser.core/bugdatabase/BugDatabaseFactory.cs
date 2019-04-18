using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core.bugdatabase
{
    public class BugDatabaseFactory : IBugDatabaseFactory
    {
        public IHttpClientWrapper GetHttpClientWrapper()
        {
            return new HttpClientWrapper();
        }

        public _Assembly LoadFile(string file)
        {
            return Assembly.LoadFile(file);
        }

        public IBugDatabaseProvider CreateInstance(Type type)
        {
            return Activator.CreateInstance(type) as IBugDatabaseProvider;
        }
    }
}
