using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core.bugdatabase;

namespace vcsparser.core
{
    public class BugDatabaseProcessor
    {
        private readonly ILogger logger;

        public BugDatabaseProcessor(ILogger logger)
        {
            this.logger = logger;
        }

        private BugDatabaseProvider LoadDllFromPath(string dllPath)
        {
            try
            {
                var DLL = Assembly.LoadFile(dllPath);
                IEnumerable<Type> validTypes = DLL.GetExportedTypes().Where((type) => typeof(BugDatabaseProvider).IsAssignableFrom(type));
                if (!validTypes.Any())
                {
                    logger.LogToConsole($"Dll must contain a public implementation of '{typeof(BugDatabaseProvider)}'");
                    return null;
                }
                else if (validTypes.Count() > 1)
                {
                    logger.LogToConsole($"Dll can only contain one public implementation of '{typeof(BugDatabaseProvider)}'. Found {validTypes.Count()}");
                    return null;
                }

                BugDatabaseProvider databaseProvider = Activator.CreateInstance(validTypes.First(), logger) as BugDatabaseProvider;
                return databaseProvider;
            }
            catch (Exception e)
            {
                logger.LogToConsole(e.Message);
                return null;
            }
        }

        public int Process(BugDatabaseLineArgs a)
        {
            var path = Path.GetFullPath(a.DLL);
            BugDatabaseProvider databaseProvider = LoadDllFromPath(path);
            if (databaseProvider == null)
            {
                logger.LogToConsole($"Unable to load Dll {path}");
                return 1;
            }
            var parsed = databaseProvider.ProcessArgs(a.DllArgs.ToArray());
            if (parsed != 0)
            {
                logger.LogToConsole("Unable to parse Dll arguments. Check requirements");
                return parsed;
            }
            try
            {
                var list = databaseProvider.Run();
                // TODO How do we save this (file name, date, etc.)
                Console.WriteLine(JsonConvert.SerializeObject(list));
            }
            catch (Exception e)
            {
                logger.LogToConsole(e.Message);
                return 1;
            }
            return 0;
        }
    }
}
