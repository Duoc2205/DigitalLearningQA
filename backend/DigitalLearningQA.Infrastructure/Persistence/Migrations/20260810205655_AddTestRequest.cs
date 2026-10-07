using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalLearningQA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTestRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TestRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaterialVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TesterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Conclusion = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestRequests_MaterialVersions_MaterialVersionId",
                        column: x => x.MaterialVersionId,
                        principalTable: "MaterialVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestRequests_MaterialVersionId",
                table: "TestRequests",
                column: "MaterialVersionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestRequests");
        }
    }
}
