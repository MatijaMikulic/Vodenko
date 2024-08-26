using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdatingTablesForeignKeyRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Dynamic_Data",
                table: "Dynamic_Data");

            migrationBuilder.RenameTable(
                name: "Dynamic_Data",
                newName: "DynamicData");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DynamicData",
                table: "DynamicData",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_DynamicData",
                table: "DynamicData");

            migrationBuilder.RenameTable(
                name: "DynamicData",
                newName: "Dynamic_Data");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Dynamic_Data",
                table: "Dynamic_Data",
                column: "Id");
        }
    }
}
