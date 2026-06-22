using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AgentWorkflow.Core.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskActivityHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "task_activities",
                columns: table => new
                {
                    Sequence = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    WorkflowRunId = table.Column<Guid>(type: "uuid", nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Summary = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_activities", x => x.Sequence);
                    table.ForeignKey(
                        name: "FK_task_activities_workflow_runs_WorkflowRunId",
                        column: x => x.WorkflowRunId,
                        principalTable: "workflow_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_task_activities_Id",
                table: "task_activities",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_task_activities_TaskId_Sequence",
                table: "task_activities",
                columns: new[] { "TaskId", "Sequence" });

            migrationBuilder.CreateIndex(
                name: "IX_task_activities_WorkflowRunId",
                table: "task_activities",
                column: "WorkflowRunId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "task_activities");
        }
    }
}
