using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MangaBook.Data.Migrations
{
    public partial class update_col_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GenreId",
                table: "MangaInGenres");

            migrationBuilder.UpdateData(
                table: "AppRoles",
                keyColumn: "Id",
                keyValue: new Guid("8d04dce2-969a-435d-bba4-df3f325983dc"),
                column: "ConcurrencyStamp",
                value: "500f7a28-caed-4a04-b1ee-d57828d09f72");

            migrationBuilder.UpdateData(
                table: "AppUsers",
                keyColumn: "Id",
                keyValue: new Guid("69bd714f-9576-45ba-b5b7-f00649be00de"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "9c8db1e3-f469-4dba-8c20-9745f9da3dbc", "AQAAAAEAACcQAAAAEDzWS181vBlQbmluEa9Y4ZkRiQ0zC5+fDa198/LUKIYGEB/x/F7Pk4zL2wlILQngvQ==" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GenreId",
                table: "MangaInGenres",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

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
    }
}
