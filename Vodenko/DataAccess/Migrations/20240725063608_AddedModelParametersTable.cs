using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddedModelParametersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ModelParameters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Theta1 = table.Column<double>(type: "float", nullable: false),
                    Theta2 = table.Column<double>(type: "float", nullable: false),
                    Theta3 = table.Column<double>(type: "float", nullable: false),
                    Theta4 = table.Column<double>(type: "float", nullable: false),
                    Theta5 = table.Column<double>(type: "float", nullable: false),
                    Theta6 = table.Column<double>(type: "float", nullable: false),
                    Theta7 = table.Column<double>(type: "float", nullable: false),
                    dateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelParameters", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModelParameters");
        }
    }
}
