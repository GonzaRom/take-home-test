namespace Fundo.Applications.WebApi.Configuration
{
    public sealed class DatabaseSettings
    {
        public const string SectionName = "Database";

        public bool AutoMigrate { get; set; }

        public int MigrationMaxAttempts { get; set; }

        public int MigrationRetryDelaySeconds { get; set; }
    }
}
