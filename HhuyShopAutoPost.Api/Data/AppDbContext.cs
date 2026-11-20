using Microsoft.EntityFrameworkCore;
using HhuyShopAutoPost.Api.Models;

namespace HhuyShopAutoPost.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // CÁC BẢNG CỦA PHASE 1 (Giữ nguyên không đụng chạm)
        public DbSet<KichBanDangBai> KichBanDangBai { get; set; }
        public DbSet<CommentMau> CommentMaus { get; set; }
        public DbSet<CauHinh> CauHinhs { get; set; }
        public DbSet<NguoiDung> NguoiDungs { get; set; }
        public DbSet<TaiKhoanClone> TaiKhoanClone { get; set; }
        public DbSet<KichBanSeeding> KichBanSeeding { get; set; }
        public DbSet<TuKhoaTheoDoi> TuKhoaTheoDois { get; set; }
        public DbSet<DuLieuLangNghe> DuLieuLangNghes { get; set; }
        public DbSet<TinNhan> TinNhans { get; set; } // Bảng Tin nhắn Facebook cũ

        // =======================================================
        // PHASE 2: CÁC BẢNG DỮ LIỆU CHO TÍNH NĂNG CHATBOX ĐA KÊNH
        // =======================================================
        public DbSet<KhachHangDaKenh> KhachHangDaKenhs { get; set; }
        public DbSet<CuocHoiThoai> CuocHoiThoais { get; set; }
        public DbSet<TinNhanDaKenh> TinNhanDaKenhs { get; set; } // Bảng Tin nhắn Đa kênh mới
    }
}