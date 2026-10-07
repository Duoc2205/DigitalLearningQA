using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalLearningQA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMaterialVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Remark",
                table: "MaterialVersions",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "MaterialVersions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "MaterialVersions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedAt",
                table: "MaterialVersions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaterialVersions_UploadedBy",
                table: "MaterialVersions",
                column: "UploadedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_MaterialVersions_Users_UploadedBy",
                table: "MaterialVersions",
                column: "UploadedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaterialVersions_Users_UploadedBy",
                table: "MaterialVersions");

            migrationBuilder.DropIndex(
                name: "IX_MaterialVersions_UploadedBy",
                table: "MaterialVersions");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "MaterialVersions");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "MaterialVersions");

            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                table: "MaterialVersions");

            migrationBuilder.AlterColumn<string>(
                name: "Remark",
                table: "MaterialVersions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);
        }
    }
}
