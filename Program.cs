using LibraryManagement.Models;
using LibraryManagement.Utilities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.IO;

var builder = WebApplication.CreateBuilder(args);
var connection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<DataContext>(options => options.UseSqlServer(connection));

// Cung cấp HttpContextAccessor để truy cập Cookie Claims an toàn
builder.Services.AddHttpContextAccessor();

// Cấu hình Cookie Authentication & Phân quyền
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "LibraryManagement.AuthCookie";
        options.LoginPath = "/Admin/Login";
        options.LogoutPath = "/Admin/Home/Logout";
        options.AccessDeniedPath = "/Admin/Login/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Cấu hình HttpContextAccessor cho helper Functions
Functions.Configure(app.Services.GetRequiredService<IHttpContextAccessor>());

// Tự động kiểm tra & bổ sung các cột mã định danh nếu chưa có trong CSDL
try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<DataContext>();
        db.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('tblUser') AND name = 'UserCode')
                ALTER TABLE [dbo].[tblUser] ADD [UserCode] NVARCHAR(30) NULL;

            IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('tblUser') AND name = 'Password' AND max_length < 500 AND max_length > 0)
                ALTER TABLE [dbo].[tblUser] ALTER COLUMN [Password] NVARCHAR(255) NOT NULL;

            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('tblBook') AND name = 'BookCode')
                ALTER TABLE [dbo].[tblBook] ADD [BookCode] NVARCHAR(30) NULL;

            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('tblBorrowTicket') AND name = 'TicketCode')
                ALTER TABLE [dbo].[tblBorrowTicket] ADD [TicketCode] NVARCHAR(30) NULL;

            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('tblBorrowTicket') AND name = 'ReturnCode')
                ALTER TABLE [dbo].[tblBorrowTicket] ADD [ReturnCode] NVARCHAR(30) NULL;

            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('tblBorrowDetail') AND name = 'IsReturned')
                ALTER TABLE [dbo].[tblBorrowDetail] ADD [IsReturned] BIT NOT NULL DEFAULT 0;

            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('tblBorrowDetail') AND name = 'ReturnDate')
                ALTER TABLE [dbo].[tblBorrowDetail] ADD [ReturnDate] DATETIME NULL;

            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('tblBorrowDetail') AND name = 'Note')
                ALTER TABLE [dbo].[tblBorrowDetail] ADD [Note] NVARCHAR(255) NULL;

            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('tblPublisher') AND name = 'IsActive')
                ALTER TABLE [dbo].[tblPublisher] ADD [IsActive] BIT NOT NULL DEFAULT 1;

            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('tblAuthor') AND name = 'IsActive')
                ALTER TABLE [dbo].[tblAuthor] ADD [IsActive] BIT NOT NULL DEFAULT 1;

            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('tblAuthor') AND name = 'Bio')
                ALTER TABLE [dbo].[tblAuthor] ADD [Bio] NVARCHAR(500) NULL;

            IF OBJECT_ID('tblOverdueWarning', 'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[tblOverdueWarning] (
                    [WarningID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    [TicketID] INT NOT NULL,
                    [UserID] INT NOT NULL,
                    [OverdueDays] INT NOT NULL DEFAULT 1,
                    [SeverityLevel] NVARCHAR(30) NOT NULL DEFAULT N'Nhắc nhở',
                    [OverdueBookCount] INT NOT NULL DEFAULT 1,
                    [BookTitles] NVARCHAR(1000) NULL,
                    [WarningStatus] NVARCHAR(50) NOT NULL DEFAULT N'Chưa nhắc nhở',
                    [ReminderCount] INT NOT NULL DEFAULT 0,
                    [LastRemindedDate] DATETIME NULL,
                    [RemindedBy] NVARCHAR(100) NULL,
                    [ReminderNote] NVARCHAR(500) NULL,
                    [ResolvedDate] DATETIME NULL,
                    [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
                    [UpdatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
                    CONSTRAINT [UQ_tblOverdueWarning_TicketID] UNIQUE ([TicketID])
                );
            END
        ");
    }
}
catch
{
    // Bỏ qua nếu CSDL chưa sẵn sàng trong lúc build hoặc thiết lập
}

// Tự động nạp dữ liệu mẫu CHỈ KHI ở môi trường Development/Demo và CSDL hoàn toàn trống
if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("EnableDemoDataSeeder"))
{
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<DataContext>();
            DevelopmentDataSeeder.SeedIfEmpty(db);
        }
    }
    catch { }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "uploads")
    ),
    RequestPath = "/files"
});

app.UseRouting();

// Cookie Authentication phải đứng TRƯỚC UseAuthorization
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
