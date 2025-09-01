using BingoGameOnline.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6; // or your preferred minimum length
})
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();
builder.Services.AddSession();
builder.Services.AddSignalR();
builder.Services.AddSingleton<BingoGameOnline.Server.Hubs.IRoomHubNotifier, BingoGameOnline.Server.Hubs.RoomHubNotifier>();
builder.Services.AddHostedService<BingoGameOnline.Server.Services.UnconfirmedAccountCleanupService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseSession();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Remove("Expires");
        ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=3600");
    }
});
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();
app.MapHub<BingoGameOnline.Server.Hubs.RoomHub>("/roomhub");
app.MapHub<BingoGameOnline.Server.Hubs.ChatHub>("/chathub");
app.MapHub<BingoGameOnline.Server.Hubs.PresenceHub>("/presencehub");
app.MapHub<BingoGameOnline.Server.Hubs.FriendsHub>("/friendshub");
app.MapHub<BingoGameOnline.Server.Hubs.UserChatHub>("/userchathub");
app.MapControllers();

app.Run();
