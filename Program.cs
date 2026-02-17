using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TRPGLogArrangeTool.Blazor;

// WebAssembly アプリケーションのホストを構成します
var builder = WebAssemblyHostBuilder.CreateDefault(args);
// メインコンポーネントとヘッド要素の追加
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient の登録（ベースアドレスをホストのアドレスに設定）
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
// 画像管理サービス (Singleton) の登録
builder.Services.AddSingleton<TRPGLogArrangeTool.Blazor.Services.ImageService>();
// ログ解析サービス (Scoped) の登録
builder.Services.AddScoped<TRPGLogArrangeTool.Blazor.Services.LogArrangeService>();

// アプリケーションの構築と実行
await builder.Build().RunAsync();
