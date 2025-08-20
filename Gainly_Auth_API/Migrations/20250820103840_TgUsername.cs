using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gainly_Auth_API.Migrations
{
    /// <inheritdoc />
    public partial class TgUsername : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TGUsername",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TGUsername",
                table: "Users");
        }
    }
}
