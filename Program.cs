using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TRPGLogArrangeTool.Blazor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<TRPGLogArrangeTool.Blazor.Services.ImageService>();
builder.Services.AddScoped<TRPGLogArrangeTool.Blazor.Services.LogArrangeService>();

await builder.Build().RunAsync();
