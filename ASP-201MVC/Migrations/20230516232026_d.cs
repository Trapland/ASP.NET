using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASP_201MVC.Migrations
{
    /// <inheritdoc />
    public partial class d : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Posts_ReplyId",
                table: "Posts",
                column: "ReplyId");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Posts_Posts_ReplyId",
            //    table: "Posts",
            //    column: "ReplyId",
            //    principalTable: "Posts",
            //    principalColumn: "Id");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Rates_Sections_ItemId",
            //    table: "Rates",
            //    column: "ItemId",
            //    principalTable: "Sections",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_Posts_Posts_ReplyId",
            //    table: "Posts");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_Rates_Sections_ItemId",
            //    table: "Rates");

            migrationBuilder.DropIndex(
                name: "IX_Posts_ReplyId",
                table: "Posts");
        }
    }
}
