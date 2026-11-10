using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chortke.Data.Migrations
{
    /// <inheritdoc />
    public partial class ImproveModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Articles_Books_BookId",
                table: "Articles");

            migrationBuilder.DropForeignKey(
                name: "FK_Articles_Documents_DocumentId",
                table: "Articles");

            migrationBuilder.DropForeignKey(
                name: "FK_Books_Books_ParentId",
                table: "Books");

            migrationBuilder.AddColumn<string>(
                name: "Tag",
                table: "Articles",
                type: "TEXT",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Articles_Books_BookId",
                table: "Articles",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Articles_Documents_DocumentId",
                table: "Articles",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Books_Books_ParentId",
                table: "Books",
                column: "ParentId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Articles_Books_BookId",
                table: "Articles");

            migrationBuilder.DropForeignKey(
                name: "FK_Articles_Documents_DocumentId",
                table: "Articles");

            migrationBuilder.DropForeignKey(
                name: "FK_Books_Books_ParentId",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "Tag",
                table: "Articles");

            migrationBuilder.AddForeignKey(
                name: "FK_Articles_Books_BookId",
                table: "Articles",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Articles_Documents_DocumentId",
                table: "Articles",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Books_Books_ParentId",
                table: "Books",
                column: "ParentId",
                principalTable: "Books",
                principalColumn: "Id");
        }
    }
}
