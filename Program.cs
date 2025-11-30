using Microsoft.AspNetCore.Authentication.Cookies;
using MyBlog.Repository.Context;
using Microsoft.EntityFrameworkCore;
using MyBlog.Repository.Interfaces;
using MyBlog.Services;

namespace MyBlog;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") 
                               ?? builder.Configuration.GetConnectionString("DefaultConnection");
        
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));
        
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IPostService, PostService>();
        builder.Services.AddScoped<ICommentService, CommentService>();
        builder.Services.AddScoped<IAdminService, AdminService>();
        builder.Services.AddScoped<ILikeService, LikeService>();
        
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Login";
                options.LogoutPath = "/Logout";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.AccessDeniedPath = "/AccessDenied";
            });
        
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => 
                policy.RequireClaim(System.Security.Claims.ClaimTypes.Role, "2"));
        });

        builder.Services.AddRazorPages();
        builder.Services.AddHttpClient();
        
        var app = builder.Build();
        
        var port = Environment.GetEnvironmentVariable("PORT") ?? "8081";
        app.Urls.Add($"http://0.0.0.0:{port}");
        
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapStaticAssets();
        app.MapRazorPages().WithStaticAssets();
        
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
        }
        
        app.Run();
    }
}