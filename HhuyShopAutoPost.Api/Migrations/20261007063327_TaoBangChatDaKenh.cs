using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HhuyShopAutoPost.Api.Migrations
{
    /// <inheritdoc />
    public partial class TaoBangChatDaKenh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CuocHoiThoaiId",
                table: "TinNhan",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "KhachHangDaKenhs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenKhachHang = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnhDaiDien = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NenTang = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlatformUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhachHangDaKenhs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CuocHoiThoais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KhachHangId = table.Column<int>(type: "int", nullable: false),
                    ThreadId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThoiGianCapNhatCuoi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuocHoiThoais", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CuocHoiThoais_KhachHangDaKenhs_KhachHangId",
                        column: x => x.KhachHangId,
                        principalTable: "KhachHangDaKenhs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TinNhan_CuocHoiThoaiId",
                table: "TinNhan",
                column: "CuocHoiThoaiId");

            migrationBuilder.CreateIndex(
                name: "IX_CuocHoiThoais_KhachHangId",
                table: "CuocHoiThoais",
                column: "KhachHangId");

            migrationBuilder.AddForeignKey(
                name: "FK_TinNhan_CuocHoiThoais_CuocHoiThoaiId",
                table: "TinNhan",
                column: "CuocHoiThoaiId",
                principalTable: "CuocHoiThoais",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TinNhan_CuocHoiThoais_CuocHoiThoaiId",
                table: "TinNhan");

            migrationBuilder.DropTable(
                name: "CuocHoiThoais");

            migrationBuilder.DropTable(
                name: "KhachHangDaKenhs");

            migrationBuilder.DropIndex(
                name: "IX_TinNhan_CuocHoiThoaiId",
                table: "TinNhan");

            migrationBuilder.DropColumn(
                name: "CuocHoiThoaiId",
                table: "TinNhan");
        }
    }
}
