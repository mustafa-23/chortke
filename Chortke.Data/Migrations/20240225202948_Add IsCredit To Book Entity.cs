using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chortke.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsCreditToBookEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool?>(
                name: "IsCredit",
                table: "Books",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCredit",
                table: "Books");
        }
    }
}
