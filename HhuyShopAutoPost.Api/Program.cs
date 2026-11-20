using Microsoft.EntityFrameworkCore;
using HhuyShopAutoPost.Api.Data;
using HhuyShopAutoPost.Api.Services;
using HhuyShopAutoPost.Api.Hubs; // Khai báo thư viện Hubs

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// 1. Thêm cấu hình CORS (Đã cập nhật để SignalR đi qua được)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp",
        policy => policy.SetIsOriginAllowed(_ => true) // Mở khóa cho tất cả các cổng kết nối
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()); // SignalR bắt buộc phải có dòng này
});

// Bật Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Kết nối SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Đăng ký HttpClientFactory để tối ưu hóa việc gọi API ra bên ngoài
builder.Services.AddHttpClient();

// Đăng ký Service Seeding để Controller có thể gọi
builder.Services.AddScoped<FacebookSeedingService>();

// ---- ĐÂY LÀ CÔNG TẮC BẬT LÍNH GÁC CHẠY NGẦM ----
builder.Services.AddHostedService<AutoPostBackgroundService>();

// THÊM DÒNG NÀY ĐỂ BẬT BOT LẮNG NGHE MXH[cite: 16]
builder.Services.AddHostedService<SocialListeningBackgroundService>();

// Đăng ký dịch vụ SignalR cho Real-time
builder.Services.AddSignalR();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// 2. Kích hoạt CORS (Lưu ý: Phải đặt trước UseAuthorization)
app.UseCors("AllowVueApp");

app.UseAuthorization();
app.MapControllers();

// Cấu hình đường ống kết nối SignalR
app.MapHub<ChatHub>("/chathub");

app.Run();