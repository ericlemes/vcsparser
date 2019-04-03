using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using vcsparser.bugdatabase;

namespace vcsparser.core
{
    public class BugDatabaseProcessor
    {
        private ILogger logger;

        public BugDatabaseProcessor(ILogger logger)
        {
            this.logger = logger;
        }

        public void Process(BugDatabaseLineArgs a)
        {
            var DLL = Assembly.LoadFile(a.DLL);
            var type = DLL.GetType(@"vcsparser.bugdatabase.BugDatabaseProvider");
            IBugDatabaseProvider databaseProvider = Activator.CreateInstance(type) as IBugDatabaseProvider;
            if (databaseProvider == null)
            {
                logger.LogToConsole("Could not load dll");
                return;
            }

            var parsed = databaseProvider.ProcessArgs(a.DllArgs.ToArray());
            if (parsed)
            {
                try
                {
                    var list = databaseProvider.Run();
                    Console.WriteLine(JsonConvert.SerializeObject(list));
                }
                catch (Exception e)
                {
                    logger.LogToConsole(e.Message);
                }
            }
            else
            {
                logger.LogToConsole("Unable to dll parse arguments. Check requirements");
            }
        }
    }
}
