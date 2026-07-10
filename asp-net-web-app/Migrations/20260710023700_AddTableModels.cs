using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace asp_net_web_app.Migrations
{
    /// <inheritdoc />
    public partial class AddTableModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "emailAddress",
                table: "Users",
                newName: "zip");

            migrationBuilder.RenameColumn(
                name: "address",
                table: "Users",
                newName: "streetAddress");

            migrationBuilder.RenameColumn(
                name: "ReservationId",
                table: "Reservations",
                newName: "reservationId");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Users",
                type: "TEXT",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "city",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "isAdmin",
                table: "Users",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "isBanned",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "isCurrentEmployee",
                table: "Users",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "state",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<decimal>(
                name: "totalCost",
                table: "Reservations",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    paymentId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    paidAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    stripeId = table.Column<string>(type: "TEXT", nullable: false),
                    paymentStatus = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.paymentId);
                });

            migrationBuilder.CreateTable(
                name: "Pricing",
                columns: table => new
                {
                    pricingId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    baseNightlyRate = table.Column<decimal>(type: "TEXT", nullable: false),
                    seasonMultiplier = table.Column<decimal>(type: "TEXT", nullable: false),
                    largeSiteMultiplier = table.Column<decimal>(type: "TEXT", nullable: false),
                    utilityMultiplier = table.Column<decimal>(type: "TEXT", nullable: false),
                    lastUpdated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    cancellationFee = table.Column<decimal>(type: "TEXT", nullable: false),
                    earlyCheckInFee = table.Column<decimal>(type: "TEXT", nullable: false),
                    lateCheckOutFee = table.Column<decimal>(type: "TEXT", nullable: false),
                    specialEventMultiplier = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pricing", x => x.pricingId);
                });

            migrationBuilder.CreateTable(
                name: "Sites",
                columns: table => new
                {
                    siteId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    length = table.Column<double>(type: "REAL", nullable: false),
                    width = table.Column<double>(type: "REAL", nullable: false),
                    isAvailable = table.Column<bool>(type: "INTEGER", nullable: false),
                    location = table.Column<string>(type: "TEXT", nullable: false),
                    Discriminator = table.Column<string>(type: "TEXT", maxLength: 21, nullable: false),
                    utilities = table.Column<string>(type: "TEXT", nullable: true),
                    height = table.Column<double>(type: "REAL", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sites", x => x.siteId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Pricing");

            migrationBuilder.DropTable(
                name: "Sites");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "city",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "email",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "isAdmin",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "isBanned",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "isCurrentEmployee",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "state",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "zip",
                table: "Users",
                newName: "emailAddress");

            migrationBuilder.RenameColumn(
                name: "streetAddress",
                table: "Users",
                newName: "address");

            migrationBuilder.RenameColumn(
                name: "reservationId",
                table: "Reservations",
                newName: "ReservationId");

            migrationBuilder.AlterColumn<int>(
                name: "totalCost",
                table: "Reservations",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "TEXT");
        }
    }
}
