using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BingoGameOnline.Client;
using BingoGameOnline.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<BingoGameOnline.Client.Services.BingoHubService>();
builder.Services.AddScoped<BingoGameOnline.Client.Services.AdminBingoHubService>();

await builder.Build().RunAsync();
