using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalWallet.Migrations
{
    /// <inheritdoc />
    public partial class Update_TransactionEvents_EventData_EventType_Conversion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "EventData_EventType",
                table: "transaction_events",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "EventData_EventType",
                table: "transaction_events",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
