using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Moka.Sdk.SqlLite
{
    public class MokaClientContext : DbContext
    {
        public const string DEFAULTDBFILE = EnvConsts.DEFAULTDBFILE;
        public DbSet<MessageLite> Messages { get; set; }
        public DbSet<UserLite> Users { get; set; }
        
        private readonly string dbFile = DEFAULTDBFILE;
        private SqliteConnection connection;
        public MokaClientContext(){}

        public MokaClientContext(string databaseFile)
        {
            if(!string.IsNullOrEmpty(databaseFile)) dbFile = databaseFile;
        }
        public MokaClientContext(SqliteConnection sqliteConnection)
        {
            if (!string.IsNullOrEmpty(sqliteConnection?.DataSource)) dbFile = sqliteConnection.DataSource;
            connection = sqliteConnection;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            connection ??= InitializeSQLiteConnection(dbFile);
            optionsBuilder.UseSqlite(connection);
        }

        private static SqliteConnection InitializeSQLiteConnection(string databaseFile)
        {
            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = databaseFile,
                Password = "Test123"// PRAGMA key is being sent from EF Core directly after opening the connection
            };
            return new SqliteConnection(connectionString.ToString());
        }
    }
}