using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Relay.Infrastructure.Migrations
{
    public partial class LoadSeedData : Migration
    {
        public const string SeedScriptResourceName = "Relay.Infrastructure.Seed.seed.sql";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(ReadSeedScript());
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM activity_events;");
            migrationBuilder.Sql("DELETE FROM accounts;");
        }

        private static string ReadSeedScript()
        {
            using var seedScriptStream = typeof(LoadSeedData).Assembly.GetManifestResourceStream(SeedScriptResourceName)
                ?? throw new InvalidOperationException($"Embedded resource '{SeedScriptResourceName}' (db/seed.sql) is missing from {typeof(LoadSeedData).Assembly.GetName().Name}.");
            using var seedScriptReader = new StreamReader(seedScriptStream);
            return seedScriptReader.ReadToEnd();
        }
    }
}
