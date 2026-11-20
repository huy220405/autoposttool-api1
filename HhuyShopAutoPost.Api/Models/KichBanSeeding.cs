using System.ComponentModel.DataAnnotations;

namespace HhuyShopAutoPost.Api.Models
{
    public class KichBanSeeding
    {
        [Key]
        public int Id { get; set; }
        public int KichBanDangBaiId { get; set; }
        public string FacebookPostId { get; set; }
        public string DanhSachComment { get; set; }
        public int DelayMin { get; set; } = 30;
        public int DelayMax { get; set; } = 90;
        public string TrangThai { get; set; } = "CHO_CHAY";
    }
}