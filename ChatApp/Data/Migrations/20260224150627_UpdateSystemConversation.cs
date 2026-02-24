using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSystemConversation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SystemConversationId",
                table: "ChatMessages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SystemConversations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsGroupChat = table.Column<bool>(type: "bit", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemConversations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemConversationParticipants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SystemConversationId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HasLeft = table.Column<bool>(type: "bit", nullable: false),
                    LeftAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemConversationParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemConversationParticipants_SystemConversations_SystemConversationId",
                        column: x => x.SystemConversationId,
                        principalTable: "SystemConversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_SystemConversationId",
                table: "ChatMessages",
                column: "SystemConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemConversationParticipants_SystemConversationId",
                table: "SystemConversationParticipants",
                column: "SystemConversationId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_SystemConversations_SystemConversationId",
                table: "ChatMessages",
                column: "SystemConversationId",
                principalTable: "SystemConversations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_SystemConversations_SystemConversationId",
                table: "ChatMessages");

            migrationBuilder.DropTable(
                name: "SystemConversationParticipants");

            migrationBuilder.DropTable(
                name: "SystemConversations");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_SystemConversationId",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "SystemConversationId",
                table: "ChatMessages");
        }
    }
}
