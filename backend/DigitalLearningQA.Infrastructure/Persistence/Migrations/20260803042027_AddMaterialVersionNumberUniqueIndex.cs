using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalLearningQA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterialVersionNumberUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MaterialVersions_LearningMaterialId",
                table: "MaterialVersions");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialVersions_LearningMaterialId_VersionNumber",
                table: "MaterialVersions",
                columns: new[] { "LearningMaterialId", "VersionNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MaterialVersions_LearningMaterialId_VersionNumber",
                table: "MaterialVersions");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialVersions_LearningMaterialId",
                table: "MaterialVersions",
                column: "LearningMaterialId");
        }
    }
}
