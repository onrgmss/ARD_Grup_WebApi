using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARD_Grup_WebApi.Migrations
{
    public partial class UpdatedPhotoAdding : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfilePhotoPath",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfilePhotoPath",
                table: "Users");
        }
    }
}
