using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MangaBook.Data.Migrations
{
    public partial class fix_value : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                columns: new[] { "ConcurrencyStamp", "NormalizedUserName", "PasswordHash" },
                values: new object[] { "874ca463-c210-480c-9238-1062ec5bd86c", "USER1", "AQAAAAEAACcQAAAAELHwBoU61w8jSR8tOYExYt50RUSlkxNtk/5D/1Zv5jY0VFMxM2G2UnKZ1mEjosR+jg==" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AppRoles",
                keyColumn: "Id",
                keyValue: new Guid("8d04dce2-969a-435d-bba4-df3f325983dc"),
                column: "ConcurrencyStamp",
                value: "8939afa1-b8d8-4b0e-8a31-7bf56a15ec2c");

            migrationBuilder.UpdateData(
                table: "AppUsers",
                keyColumn: "Id",
                keyValue: new Guid("69bd714f-9576-45ba-b5b7-f00649be00de"),
                columns: new[] { "ConcurrencyStamp", "NormalizedUserName", "PasswordHash" },
                values: new object[] { "de04d118-6c91-49a8-8d1f-95fccba3d5dc", "user1", "AQAAAAEAACcQAAAAEGHoSb8j4/pKB3QtsTVoUdgKUYliGtc8VsZhjIOXWOqbLhWKPZEiSMZ7vvg9mQ3XsQ==" });
        }
    }
}
