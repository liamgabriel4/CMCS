using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMCS.Data.Migrations
{
    /// <inheritdoc />
    public partial class addedhr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LecturerId",
                table: "Claims",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LecturerId",
                table: "Claims");
        }
    }
}
