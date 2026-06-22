using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgentWorkflow.Core.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovalPolicyEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "approvals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TaskId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    WorkflowRunId = table.Column<Guid>(type: "uuid", nullable: true),
                    Gate = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ArtifactHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    TargetBranch = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CommitSha = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ApprovedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    InvalidatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    InvalidationReason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_approvals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_approvals_projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_approvals_workflow_runs_WorkflowRunId",
                        column: x => x.WorkflowRunId,
                        principalTable: "workflow_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_approvals_ProjectId_TaskId_Gate_ApprovedAt",
                table: "approvals",
                columns: new[] { "ProjectId", "TaskId", "Gate", "ApprovedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_approvals_WorkflowRunId",
                table: "approvals",
                column: "WorkflowRunId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "approvals");
        }
    }
}
