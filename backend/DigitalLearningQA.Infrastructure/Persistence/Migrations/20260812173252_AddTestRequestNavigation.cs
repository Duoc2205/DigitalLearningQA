using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalLearningQA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTestRequestNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Conclusion",
                table: "TestRequests",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TestRequests_TesterId",
                table: "TestRequests",
                column: "TesterId");

            migrationBuilder.AddForeignKey(
                name: "FK_TestRequests_Users_TesterId",
                table: "TestRequests",
                column: "TesterId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestRequests_Users_TesterId",
                table: "TestRequests");

            migrationBuilder.DropIndex(
                name: "IX_TestRequests_TesterId",
                table: "TestRequests");

            migrationBuilder.AlterColumn<string>(
                name: "Conclusion",
                table: "TestRequests",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);
        }
    }
}
