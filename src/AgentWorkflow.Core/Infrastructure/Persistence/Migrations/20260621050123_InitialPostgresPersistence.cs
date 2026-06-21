using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgentWorkflow.Core.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgresPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "projects",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "workflow_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ResultJson = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engineering_tasks",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ProjectId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Priority = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engineering_tasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engineering_tasks_projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workflow_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RunId = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Agent = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workflow_events_workflow_runs_RunId",
                        column: x => x.RunId,
                        principalTable: "workflow_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "work_items",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EngineeringTaskId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Source = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Priority = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Tags = table.Column<string[]>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_work_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_work_items_engineering_tasks_EngineeringTaskId",
                        column: x => x.EngineeringTaskId,
                        principalTable: "engineering_tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_engineering_tasks_ProjectId_CreatedAt",
                table: "engineering_tasks",
                columns: new[] { "ProjectId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_work_items_EngineeringTaskId",
                table: "work_items",
                column: "EngineeringTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_work_items_Source_SourceKey",
                table: "work_items",
                columns: new[] { "Source", "SourceKey" });

            migrationBuilder.CreateIndex(
                name: "IX_workflow_events_RunId_Timestamp",
                table: "workflow_events",
                columns: new[] { "RunId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_workflow_runs_TaskId",
                table: "workflow_runs",
                column: "TaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "work_items");

            migrationBuilder.DropTable(
                name: "workflow_events");

            migrationBuilder.DropTable(
                name: "engineering_tasks");

            migrationBuilder.DropTable(
                name: "workflow_runs");

            migrationBuilder.DropTable(
                name: "projects");
        }
    }
}
