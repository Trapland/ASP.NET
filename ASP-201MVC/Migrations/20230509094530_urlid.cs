﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASP_201MVC.Migrations
{
    /// <inheritdoc />
    public partial class urlid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UrlId",
                table: "Sections",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UrlId",
                table: "Sections");
        }
    }
}
