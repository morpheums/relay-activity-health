using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Relay.Infrastructure.Migrations
{
    public partial class InitialCreate : Migration
    {
        private static readonly string[] AccountOccurredIndexColumns = ["account_id", "occurred_at"];
        private static readonly string[] AccountOccurredIndexIncludedColumns = ["location", "event_type", "duration_seconds", "outcome"];

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "varchar(120)", unicode: false, maxLength: 120, nullable: false),
                    industry = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false),
                    timezone = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "activity_events",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    account_id = table.Column<int>(type: "int", nullable: false),
                    location = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: false),
                    event_type = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    occurred_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    duration_seconds = table.Column<int>(type: "int", nullable: true),
                    outcome = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_activity_events", x => x.id);
                    table.ForeignKey(
                        name: "FK_activity_events_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_activity_events_account_occurred",
                table: "activity_events",
                columns: AccountOccurredIndexColumns)
                .Annotation("SqlServer:Include", AccountOccurredIndexIncludedColumns);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "activity_events");

            migrationBuilder.DropTable(
                name: "accounts");
        }
    }
}
