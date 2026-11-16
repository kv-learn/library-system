using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LibraryApi.Migrations
{
    /// <inheritdoc />
    public partial class AddGenreSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Genres",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("29fd1a92-8580-473b-8040-5facebf19be3"), "Romance" },
                    { new Guid("59225385-a68c-4f56-bcd9-728eff6e5ce4"), "Fiction" },
                    { new Guid("92403d88-69ac-4363-a995-d75f25208b66"), "Horror" },
                    { new Guid("a1a2215a-7db5-411c-bb13-cf7d6e8430f4"), "Science Fiction" },
                    { new Guid("e61d0e27-c76e-4fde-ab38-eb48298bebf4"), "Mystery" },
                    { new Guid("f5c5c073-cc6a-4a56-a4ff-6bbb015c9c65"), "Fantasy" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: new Guid("29fd1a92-8580-473b-8040-5facebf19be3"));

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: new Guid("59225385-a68c-4f56-bcd9-728eff6e5ce4"));

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: new Guid("92403d88-69ac-4363-a995-d75f25208b66"));

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: new Guid("a1a2215a-7db5-411c-bb13-cf7d6e8430f4"));

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: new Guid("e61d0e27-c76e-4fde-ab38-eb48298bebf4"));

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: new Guid("f5c5c073-cc6a-4a56-a4ff-6bbb015c9c65"));
        }
    }
}
