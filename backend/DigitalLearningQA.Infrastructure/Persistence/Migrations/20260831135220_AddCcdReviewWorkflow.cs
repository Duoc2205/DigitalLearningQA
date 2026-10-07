using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalLearningQA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCcdReviewWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChecklistReviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TestRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QualityCriterionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Outcome = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ReviewedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                }, constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistReviews", x => x.Id);
                    table.ForeignKey("FK_ChecklistReviews_QualityCriteria_QualityCriterionId", x => x.QualityCriterionId, "QualityCriteria", "Id", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_ChecklistReviews_TestRequests_TestRequestId", x => x.TestRequestId, "TestRequests", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_ChecklistReviews_Users_ReviewedById", x => x.ReviewedById, "Users", "Id", onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateTable(
                name: "QualityIssues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TestRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EvidenceUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolvedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ResolvedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolutionNote = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false), CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true), UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true), UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true), IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                }, constraints: table =>
                {
                    table.PrimaryKey("PK_QualityIssues", x => x.Id);
                    table.ForeignKey("FK_QualityIssues_TestRequests_TestRequestId", x => x.TestRequestId, "TestRequests", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_QualityIssues_Users_CreatedById", x => x.CreatedById, "Users", "Id", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_QualityIssues_Users_ResolvedById", x => x.ResolvedById, "Users", "Id", onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateIndex("IX_ChecklistReviews_QualityCriterionId", "ChecklistReviews", "QualityCriterionId");
            migrationBuilder.CreateIndex("IX_ChecklistReviews_ReviewedById", "ChecklistReviews", "ReviewedById");
            migrationBuilder.CreateIndex("IX_ChecklistReviews_TestRequestId_QualityCriterionId", "ChecklistReviews", new[] { "TestRequestId", "QualityCriterionId" }, unique: true);
            migrationBuilder.CreateIndex("IX_QualityIssues_CreatedById", "QualityIssues", "CreatedById");
            migrationBuilder.CreateIndex("IX_QualityIssues_ResolvedById", "QualityIssues", "ResolvedById");
            migrationBuilder.CreateIndex("IX_QualityIssues_TestRequestId", "QualityIssues", "TestRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ChecklistReviews");
            migrationBuilder.DropTable(name: "QualityIssues");
        }
    }
}
