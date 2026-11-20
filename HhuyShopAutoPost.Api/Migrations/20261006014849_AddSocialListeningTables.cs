using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HhuyShopAutoPost.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSocialListeningTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TuKhoaTheoDois",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TuKhoa = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TuKhoaTheoDois", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DuLieuLangNghes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TuKhoaId = table.Column<int>(type: "int", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nguon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LinkBaiViet = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CamXuc = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NgayThuThap = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuLieuLangNghes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DuLieuLangNghes_TuKhoaTheoDois_TuKhoaId",
                        column: x => x.TuKhoaId,
                        principalTable: "TuKhoaTheoDois",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DuLieuLangNghes_TuKhoaId",
                table: "DuLieuLangNghes",
                column: "TuKhoaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DuLieuLangNghes");

            migrationBuilder.DropTable(
                name: "TuKhoaTheoDois");
        }
    }
}
