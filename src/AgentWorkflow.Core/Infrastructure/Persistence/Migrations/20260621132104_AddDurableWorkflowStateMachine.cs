using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgentWorkflow.Core.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDurableWorkflowStateMachine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Attempt",
                table: "workflow_runs",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "FailureDetails",
                table: "workflow_runs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Stage",
                table: "workflow_runs",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "Created");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Attempt",
                table: "workflow_runs");

            migrationBuilder.DropColumn(
                name: "FailureDetails",
                table: "workflow_runs");

            migrationBuilder.DropColumn(
                name: "Stage",
                table: "workflow_runs");
        }
    }
}
