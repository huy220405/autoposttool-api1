using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HhuyShopAutoPost.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSeedingModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KichBanSeeding",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KichBanDangBaiId = table.Column<int>(type: "int", nullable: false),
                    FacebookPostId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DanhSachComment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DelayMin = table.Column<int>(type: "int", nullable: false),
                    DelayMax = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KichBanSeeding", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoanClone",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Uid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TenTaiKhoan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cookie = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccessToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayThem = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoanClone", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KichBanSeeding");

            migrationBuilder.DropTable(
                name: "TaiKhoanClone");
        }
    }
}
