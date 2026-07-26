using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace asp_net_web_app.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOldReservationModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiteModels");

            migrationBuilder.AddColumn<int>(
                name: "ReservationId",
                table: "Payments",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReservationId",
                table: "Payments");

            migrationBuilder.CreateTable(
                name: "SiteModels",
                columns: table => new
                {
                    siteId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Discriminator = table.Column<string>(type: "TEXT", maxLength: 21, nullable: false),
                    isAvailable = table.Column<bool>(type: "INTEGER", nullable: false),
                    length = table.Column<double>(type: "REAL", nullable: false),
                    location = table.Column<string>(type: "TEXT", nullable: false),
                    width = table.Column<double>(type: "REAL", nullable: false),
                    utilities = table.Column<string>(type: "TEXT", nullable: true),
                    height = table.Column<double>(type: "REAL", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteModels", x => x.siteId);
                });
        }
    }
}
