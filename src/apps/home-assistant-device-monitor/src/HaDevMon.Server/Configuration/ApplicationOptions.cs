namespace HaDevMon.Server.Configuration
{
    public sealed class ApplicationOptions
    {
        public const string SectionName = "Application";


        public required string Name { get; init; }

        public required string ServiceName { get; init; }
    }
}
