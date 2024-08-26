using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddedLinModelColumnsToDynamicData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WaterLevelTank2Model",
                table: "DynamicData",
                newName: "WaterLevelTank2NonLinModel");

            migrationBuilder.RenameColumn(
                name: "WaterLevelTank1Model",
                table: "DynamicData",
                newName: "WaterLevelTank2LinModel");

            migrationBuilder.RenameColumn(
                name: "InletFlowModel",
                table: "DynamicData",
                newName: "WaterLevelTank1NonLinModel");

            migrationBuilder.AddColumn<float>(
                name: "InletFlowLinModel",
                table: "DynamicData",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "InletFlowNonLinModel",
                table: "DynamicData",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "WaterLevelTank1LinModel",
                table: "DynamicData",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InletFlowLinModel",
                table: "DynamicData");

            migrationBuilder.DropColumn(
                name: "InletFlowNonLinModel",
                table: "DynamicData");

            migrationBuilder.DropColumn(
                name: "WaterLevelTank1LinModel",
                table: "DynamicData");

            migrationBuilder.RenameColumn(
                name: "WaterLevelTank2NonLinModel",
                table: "DynamicData",
                newName: "WaterLevelTank2Model");

            migrationBuilder.RenameColumn(
                name: "WaterLevelTank2LinModel",
                table: "DynamicData",
                newName: "WaterLevelTank1Model");

            migrationBuilder.RenameColumn(
                name: "WaterLevelTank1NonLinModel",
                table: "DynamicData",
                newName: "InletFlowModel");
        }
    }
}
