using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace vcsparser.core.bugdatabase
{
    public class BugDatabaseDllLoader : IBugDatabaseDllLoader
    {
        private readonly ILogger logger;
        private readonly IBugDatabaseFactory bugDatabaseFactory;

        public BugDatabaseDllLoader(ILogger logger, IBugDatabaseFactory bugDatabaseFactory)
        {
            this.logger = logger;
            this.bugDatabaseFactory = bugDatabaseFactory;
        }

        public IBugDatabaseProvider Load(string path, IEnumerable<string> args, IWebRequest webRequest)
        {
            _Assembly dll = bugDatabaseFactory.LoadFile(path);

            IEnumerable<Type> validTypes = dll.GetExportedTypes().Where((type) => typeof(IBugDatabaseProvider).IsAssignableFrom(type));
            if (!validTypes.Any())
                throw new Exception($"Dll must contain a public implementation of '{typeof(IBugDatabaseProvider)}'");
            else if (validTypes.Count() > 1)
                throw new Exception($"Dll can only contain one public implementation of '{typeof(IBugDatabaseProvider)}'. Found {validTypes.Count()}");

            IBugDatabaseProvider databaseProvider = bugDatabaseFactory.CreateInstance(validTypes.First());
            databaseProvider.Logger = logger;
            databaseProvider.WebRequest = webRequest;
            if (databaseProvider.ProcessArgs(args) != 0)
                throw new Exception("Unable to parse Dll arguments. Check requirements");

            return databaseProvider;
        }
    }
}
