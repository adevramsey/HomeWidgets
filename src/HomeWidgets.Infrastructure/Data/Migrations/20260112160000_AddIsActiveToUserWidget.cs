using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeWidgets.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToUserWidget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "user_widgets",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "user_widgets");
        }
    }
}
