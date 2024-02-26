using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShoppingCartUI.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddImageUrlModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImageUrl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LaptopId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageUrl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImageUrl_Laptop_LaptopId",
                        column: x => x.LaptopId,
                        principalTable: "Laptop",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImageUrl_LaptopId",
                table: "ImageUrl",
                column: "LaptopId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImageUrl");
        }
    }
}
