using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MangaBook.Data.Migrations
{
    public partial class update_col : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GenreSlug",
                table: "MangaInGenres",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AppRoles",
                keyColumn: "Id",
                keyValue: new Guid("8d04dce2-969a-435d-bba4-df3f325983dc"),
                column: "ConcurrencyStamp",
                value: "47dc1969-2fbe-488a-a695-0301c373190e");

            migrationBuilder.UpdateData(
                table: "AppUsers",
                keyColumn: "Id",
                keyValue: new Guid("69bd714f-9576-45ba-b5b7-f00649be00de"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "0dde61d2-4617-4c63-9a98-71b5abea5ce8", "AQAAAAEAACcQAAAAEIloF4qjDlBX3eHeAOZHtL7jk/i6MWrrGfn+DpiTfEyeHEWDBxVNBu0Oe7PH/FBe3A==" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GenreSlug",
                table: "MangaInGenres");

            migrationBuilder.UpdateData(
                table: "AppRoles",
                keyColumn: "Id",
                keyValue: new Guid("8d04dce2-969a-435d-bba4-df3f325983dc"),
                column: "ConcurrencyStamp",
                value: "3c43f443-9463-419c-9695-cc0ad2a2aff1");

            migrationBuilder.UpdateData(
                table: "AppUsers",
                keyColumn: "Id",
                keyValue: new Guid("69bd714f-9576-45ba-b5b7-f00649be00de"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "874ca463-c210-480c-9238-1062ec5bd86c", "AQAAAAEAACcQAAAAELHwBoU61w8jSR8tOYExYt50RUSlkxNtk/5D/1Zv5jY0VFMxM2G2UnKZ1mEjosR+jg==" });
        }
    }
}
