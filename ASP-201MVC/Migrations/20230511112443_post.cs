using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASP_201MVC.Migrations
{
    /// <inheritdoc />
    public partial class post : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Posts_AuthorId",
                table: "Posts",
                column: "AuthorId");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Posts_Users_AuthorId",
            //    table: "Posts",
            //    column: "AuthorId",
            //    principalTable: "Users",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_Posts_Users_AuthorId",
            //    table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_AuthorId",
                table: "Posts");
        }
    }
}
