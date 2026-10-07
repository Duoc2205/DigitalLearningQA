using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalLearningQA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLearningMaterial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "LearningMaterials",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedAt",
                table: "LearningMaterials",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "LearningMaterials");

            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                table: "LearningMaterials");
        }
    }
}
