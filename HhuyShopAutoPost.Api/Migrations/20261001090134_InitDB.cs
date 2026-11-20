using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HhuyShopAutoPost.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommentMaus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TuKhoa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoiDungTraLoi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentMaus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KichBanDangBai",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThoiGianDang = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PhanLoai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LinkHinhAnh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GhiChuLoi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KichBanDangBai", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NguoiDungs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDangNhap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MatKhauHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiDungs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CauHinhs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NguoiDungId = table.Column<int>(type: "int", nullable: false),
                    PageId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TenFanpage = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    AccessToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AvatarUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayKetNoi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauHinhs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CauHinhs_NguoiDungs_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalTable: "NguoiDungs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CauHinhs_NguoiDungId",
                table: "CauHinhs",
                column: "NguoiDungId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CauHinhs");

            migrationBuilder.DropTable(
                name: "CommentMaus");

            migrationBuilder.DropTable(
                name: "KichBanDangBai");

            migrationBuilder.DropTable(
                name: "NguoiDungs");
        }
    }
}
