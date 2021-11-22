namespace vcsparser.core
{
    public interface IP4CommandLineArgs : IBugDatabaseCommandLineArgs
    {
        string P4ChangesCommandLine { get; set; }

        string P4DescribeCommandLine { get; set; }
    }
}
