using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DigitalWallet.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexTransactionEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "transaction_events",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WalletId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    EventType = table.Column<int>(type: "integer", nullable: false),
                    TransactionId = table.Column<int>(type: "integer", nullable: false),
                    EventData_Amount = table.Column<decimal>(type: "numeric", nullable: true),
                    EventData_EventType = table.Column<int>(type: "integer", nullable: false),
                    EventData_Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EventData_TransactionId = table.Column<int>(type: "integer", nullable: true),
                    EventData_UserId = table.Column<string>(type: "character varying(18)", maxLength: 18, nullable: false),
                    EventData_WalletId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transaction_events", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_transaction_events_WalletId",
                table: "transaction_events",
                column: "WalletId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "transaction_events");
        }
    }
}
