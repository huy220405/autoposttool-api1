using System.ComponentModel.DataAnnotations;

namespace HhuyShopAutoPost.Api.Models
{
    public class TaiKhoanClone
    {
        [Key]
        public int Id { get; set; }
        public string Uid { get; set; }
        public string TenTaiKhoan { get; set; }
        public string Cookie { get; set; }
        public string AccessToken { get; set; }
        public string TrangThai { get; set; } = "LIVE";
        public DateTime NgayThem { get; set; } = DateTime.Now;
    }
}