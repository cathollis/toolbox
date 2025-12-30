using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hollis.Toolbox.Functions.Migrations
{
    /// <inheritdoc />
    public partial class AddTablePastebinItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Toolbox");

            migrationBuilder.CreateTable(
                name: "PastebinItems",
                schema: "Toolbox",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccessCode = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ContentStorageType = table.Column<int>(type: "int", nullable: false),
                    ContentInDb = table.Column<string>(type: "nvarchar(max)", maxLength: 16384, nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ExpiredAfter = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Expired = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PastebinItems", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PastebinItems_AccessCode",
                schema: "Toolbox",
                table: "PastebinItems",
                column: "AccessCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PastebinItems",
                schema: "Toolbox");
        }
    }
}
