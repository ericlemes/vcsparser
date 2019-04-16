using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core.bugdatabase
{
    public interface IHttpClientWrapperFactory
    {
        IHttpClientWrapper GetSingletonHttpClientWrapper();
    }

    public class HttpClientWrapperFactory : IHttpClientWrapperFactory
    {
        private static IBugDatabaseFactory bugDatabaseFactory;
        private static Lazy<IHttpClientWrapper> httpClientWrapper = new Lazy<IHttpClientWrapper>(() => bugDatabaseFactory.GetHttpClientWrapper());

        public HttpClientWrapperFactory(IBugDatabaseFactory bugDatabaseFactory)
        {
            HttpClientWrapperFactory.bugDatabaseFactory = bugDatabaseFactory;
        }

        public IHttpClientWrapper GetSingletonHttpClientWrapper()
        {
            return httpClientWrapper.Value;
        }
    }
}
