using System;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.SqlServer;

namespace Jobbr.Storage.MsSql
{
    public class JobbrMsSqlConfiguration
    {
        public string ConnectionString { get; set; }

        [Obsolete("Schema property is not used anymore. Give jobbr its own database - no schemas needed")]
        public string Schema { get; set; }

        public bool CreateTablesIfNotExists { get; set; }

        public IOrmLiteDialectProvider DialectProvider { get; set; } = new SqlServer2017OrmLiteDialectProvider();

        public TimeSpan? Retention { get; set; }

        public TimeSpan RetentionEnforcementInterval { get; set; } = TimeSpan.FromDays(1);
    }
}