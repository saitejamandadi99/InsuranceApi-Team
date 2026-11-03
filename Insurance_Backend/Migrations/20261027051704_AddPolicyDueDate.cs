using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Insurance_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPolicyDueDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "Policy",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Policy");
        }
    }
}
