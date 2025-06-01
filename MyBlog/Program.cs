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

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
        
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

        app.Run();
    }
}