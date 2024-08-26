using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddedNewColumnTarget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Flag",
                table: "DynamicData",
                newName: "IsPumpActive");

            migrationBuilder.AddColumn<float>(
                name: "Target",
                table: "DynamicData",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Target",
                table: "DynamicData");

            migrationBuilder.RenameColumn(
                name: "IsPumpActive",
                table: "DynamicData",
                newName: "Flag");
        }
    }
}
