using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace vcsparser.core.bugdatabase
{
    public interface IBugDatabaseDllLoader
    {
        IBugDatabaseProvider Load(string path, IEnumerable<string> args, IWebRequest webRequest);
    }

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
            _Assembly dll;
            try
            {
                dll = bugDatabaseFactory.LoadFile(path);
            }
            catch (Exception e)
            {
                logger.LogToConsole($"Error loading Dll. {e.Message}");
                return null;
            }
            IEnumerable<Type> validTypes = dll.GetExportedTypes().Where((type) => typeof(IBugDatabaseProvider).IsAssignableFrom(type));
            if (!validTypes.Any())
            {
                logger.LogToConsole($"Dll must contain a public implementation of '{typeof(IBugDatabaseProvider)}'");
                return null;
            }
            else if (validTypes.Count() > 1)
            {
                logger.LogToConsole($"Dll can only contain one public implementation of '{typeof(IBugDatabaseProvider)}'. Found {validTypes.Count()}");
                return null;
            }

            IBugDatabaseProvider databaseProvider = bugDatabaseFactory.CreateInstance(validTypes.First());
            databaseProvider.SetLogger(logger);
            databaseProvider.SetWebRequest(webRequest);
            int parsed = databaseProvider.ProcessArgs(args);
            if (parsed != 0)
            {
                logger.LogToConsole("Unable to parse Dll arguments. Check requirements");
                return null;
            }
            return databaseProvider;
        }
    }
}
