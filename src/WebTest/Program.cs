using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.bugdatabase.azuredevops;

namespace WebTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string organization = args[0];
            string project = args[1];
            string team = args[2];
            string personalAccessToken = args[3];
            string from = args[4];
            string to = args[5];
            AzureDevOps app = new AzureDevOps(organization, project, team, personalAccessToken, from, to);
            var data = app.Query();
            foreach (var item in data.WorkItems)
            {
                Console.WriteLine(JsonConvert.SerializeObject(item));
            }
        }
    }
}
