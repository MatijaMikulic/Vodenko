using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class changedColumnNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "sample",
                table: "DynamicData",
                newName: "Sample");

            migrationBuilder.RenameColumn(
                name: "flag",
                table: "DynamicData",
                newName: "Flag");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Sample",
                table: "DynamicData",
                newName: "sample");

            migrationBuilder.RenameColumn(
                name: "Flag",
                table: "DynamicData",
                newName: "flag");
        }
    }
}
