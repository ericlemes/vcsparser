namespace vcsparser.core
{
    public interface ICosmosCommandLineArgs
    {
        string CosmosDbKey { get; set; }

        string CosmosEndpoint { get; set; }
    }
}
