using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace asp_net_web_app.Migrations
{
    /// <inheritdoc />
    public partial class AddMiddleInitialAndAptSuite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "aptSuite",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "middleInitial",
                table: "Users",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "aptSuite",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "middleInitial",
                table: "Users");
        }
    }
}
