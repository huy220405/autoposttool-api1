using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HhuyShopAutoPost.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPageIdToKichBan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PageId",
                table: "KichBanDangBai",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PageId",
                table: "KichBanDangBai");
        }
    }
}
