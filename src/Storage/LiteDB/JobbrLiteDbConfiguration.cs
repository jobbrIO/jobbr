namespace Jobbr.Storage.LiteDB
{
    public class JobbrLiteDbConfiguration
    {
        public string ConnectionString { get; set; } = "jobbr.db";

        public TimeSpan? Retention { get; set; }

        public TimeSpan RetentionEnforcementInterval { get; set; } = TimeSpan.FromHours(12);

        public bool CreateDatabaseIfNotExists { get; set; } = true;
    }
}