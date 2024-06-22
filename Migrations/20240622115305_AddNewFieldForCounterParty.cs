using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cw9.Migrations
{
    /// <inheritdoc />
    public partial class AddNewFieldForCounterParty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_AspNetUsers_FromUserId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_AspNetUsers_ToUserId",
                table: "Transactions");

            migrationBuilder.AddColumn<string>(
                name: "CounterParty",
                table: "Transactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_AspNetUsers_FromUserId",
                table: "Transactions",
                column: "FromUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_AspNetUsers_ToUserId",
                table: "Transactions",
                column: "ToUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_AspNetUsers_FromUserId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_AspNetUsers_ToUserId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "CounterParty",
                table: "Transactions");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_AspNetUsers_FromUserId",
                table: "Transactions",
                column: "FromUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_AspNetUsers_ToUserId",
                table: "Transactions",
                column: "ToUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
