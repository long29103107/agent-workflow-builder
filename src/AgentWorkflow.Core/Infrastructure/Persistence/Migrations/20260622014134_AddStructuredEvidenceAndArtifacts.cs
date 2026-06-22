using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgentWorkflow.Core.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStructuredEvidenceAndArtifacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "agent_executions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RunId = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agent_executions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_agent_executions_workflow_runs_RunId",
                        column: x => x.RunId,
                        principalTable: "workflow_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "artifacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RunId = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentExecutionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    ContentType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_artifacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_artifacts_agent_executions_AgentExecutionId",
                        column: x => x.AgentExecutionId,
                        principalTable: "agent_executions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_artifacts_workflow_runs_RunId",
                        column: x => x.RunId,
                        principalTable: "workflow_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "evidence_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RunId = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentExecutionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Summary = table.Column<string>(type: "text", nullable: false),
                    SourceReference = table.Column<string>(type: "text", nullable: true),
                    Action = table.Column<string>(type: "text", nullable: true),
                    ToolName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ToolResult = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evidence_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_evidence_items_agent_executions_AgentExecutionId",
                        column: x => x.AgentExecutionId,
                        principalTable: "agent_executions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_evidence_items_workflow_runs_RunId",
                        column: x => x.RunId,
                        principalTable: "workflow_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_agent_executions_RunId_StartedAt",
                table: "agent_executions",
                columns: new[] { "RunId", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_artifacts_AgentExecutionId",
                table: "artifacts",
                column: "AgentExecutionId");

            migrationBuilder.CreateIndex(
                name: "IX_artifacts_RunId_CreatedAt",
                table: "artifacts",
                columns: new[] { "RunId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_evidence_items_AgentExecutionId",
                table: "evidence_items",
                column: "AgentExecutionId");

            migrationBuilder.CreateIndex(
                name: "IX_evidence_items_RunId_CreatedAt",
                table: "evidence_items",
                columns: new[] { "RunId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "artifacts");

            migrationBuilder.DropTable(
                name: "evidence_items");

            migrationBuilder.DropTable(
                name: "agent_executions");
        }
    }
}
