using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DigitalWallet.Migrations
{
    /// <inheritdoc />
    public partial class Add_InboxMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_outbox",
                table: "outbox");

            migrationBuilder.RenameTable(
                name: "outbox",
                newName: "outbox_messages");

            migrationBuilder.RenameIndex(
                name: "IX_outbox_Topic",
                table: "outbox_messages",
                newName: "IX_outbox_messages_Topic");

            migrationBuilder.RenameIndex(
                name: "IX_outbox_Key",
                table: "outbox_messages",
                newName: "IX_outbox_messages_Key");

            migrationBuilder.AlterColumn<Guid>(
                name: "CorrelationId",
                table: "wallet_transactions",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "EventData_CorrelationId",
                table: "transaction_events",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "Payload_CorrelationId",
                table: "outbox_messages",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddPrimaryKey(
                name: "PK_outbox_messages",
                table: "outbox_messages",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "inbox_messages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Topic = table.Column<string>(type: "text", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Received = table.Column<bool>(type: "boolean", nullable: false),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: true),
                    OutboxId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbox_messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_inbox_messages_outbox_messages_OutboxId",
                        column: x => x.OutboxId,
                        principalTable: "outbox_messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_inbox_messages_OutboxId",
                table: "inbox_messages",
                column: "OutboxId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inbox_messages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_outbox_messages",
                table: "outbox_messages");

            migrationBuilder.RenameTable(
                name: "outbox_messages",
                newName: "outbox");

            migrationBuilder.RenameIndex(
                name: "IX_outbox_messages_Topic",
                table: "outbox",
                newName: "IX_outbox_Topic");

            migrationBuilder.RenameIndex(
                name: "IX_outbox_messages_Key",
                table: "outbox",
                newName: "IX_outbox_Key");

            migrationBuilder.AlterColumn<Guid>(
                name: "CorrelationId",
                table: "wallet_transactions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "EventData_CorrelationId",
                table: "transaction_events",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Payload_CorrelationId",
                table: "outbox",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_outbox",
                table: "outbox",
                column: "Id");
        }
    }
}
