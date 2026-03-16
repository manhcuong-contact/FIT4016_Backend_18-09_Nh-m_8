using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CraftOutsourcing.Migrations
{
    /// <inheritdoc />
    public partial class AddProfitTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Profits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SampleOrderId = table.Column<int>(type: "int", nullable: false),
                    QuantityGood = table.Column<int>(type: "int", nullable: false),
                    QuantityDefect = table.Column<int>(type: "int", nullable: false),
                    SellingPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CostPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SalesProfit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PenaltyRevenue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalProfit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RecordDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Profits_SampleOrders_SampleOrderId",
                        column: x => x.SampleOrderId,
                        principalTable: "SampleOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Profits_SampleOrderId",
                table: "Profits",
                column: "SampleOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Profits");
        }
    }
}
