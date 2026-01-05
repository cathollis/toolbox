using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hollis.Toolbox.Functions.Migrations
{
    /// <inheritdoc />
    public partial class AlterTablePastebinItemAddConfiguredExpiredAfterRead : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ConfiguredExpiredAfterRead",
                schema: "Toolbox",
                table: "PastebinItems",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfiguredExpiredAfterRead",
                schema: "Toolbox",
                table: "PastebinItems");
        }
    }
}
