using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NICK2._0_core.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProcessingStates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "processing_states",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "text", nullable: false),
                    OpportunityId = table.Column<Guid>(type: "uuid", nullable: false),
                    OpportunityStatus = table.Column<int>(type: "integer", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_processing_states", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_processing_states_EventType_OpportunityId_OpportunityStatus",
                table: "processing_states",
                columns: new[] { "EventType", "OpportunityId", "OpportunityStatus" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "processing_states");
        }
    }
}
