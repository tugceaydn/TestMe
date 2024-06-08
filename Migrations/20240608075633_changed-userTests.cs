using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestMe.Migrations
{
    /// <inheritdoc />
    public partial class changeduserTests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_UserTests_UserTestId",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_UserTestId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "UserTestId",
                table: "Questions");

            migrationBuilder.AddColumn<List<int>>(
                name: "UserAnswers",
                table: "UserTests",
                type: "integer[]",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserAnswers",
                table: "UserTests");

            migrationBuilder.AddColumn<int>(
                name: "UserTestId",
                table: "Questions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_UserTestId",
                table: "Questions",
                column: "UserTestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_UserTests_UserTestId",
                table: "Questions",
                column: "UserTestId",
                principalTable: "UserTests",
                principalColumn: "Id");
        }
    }
}
