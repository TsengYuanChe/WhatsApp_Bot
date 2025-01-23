using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// 注入 WhatsAppService
builder.Services.AddHttpClient();
builder.Services.AddSingleton<WhatsAppService>();

// 添加控制器
builder.Services.AddControllers();

var app = builder.Build();

// 設定路由
app.MapControllers();

// 啟動應用
app.Run();