using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASP_201MVC.Migrations
{
    /// <inheritdoc />
    public partial class dd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ThemeImg",
                table: "Themes",
                type: "longtext",
                nullable: true);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropColumn(
                name: "ThemeImg",
                table: "Themes");
        }
    }
}
