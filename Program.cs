using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SiteLoja.Data;
using SiteLoja.Interface;
using SiteLoja.Services;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// =======================
// 🌎 Configuração de Cultura
// =======================
var defaultCulture = new CultureInfo("pt-BR");

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture(defaultCulture);
    options.SupportedCultures = new List<CultureInfo> { defaultCulture };
    options.SupportedUICultures = new List<CultureInfo> { defaultCulture };
});

// =======================
// 💾 Banco de Dados
// =======================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// =======================
// 🧩 Serviços
// =======================
builder.Services.AddScoped<IUsuario, UsuarioService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<ProdutoService>();
builder.Services.AddScoped<IFreteService,FreteService>();
builder.Services.AddScoped<IPedidoService, PedidoService>();

// =======================
// 🔐 Autenticação via Cookie
// =======================
builder.Services.AddAuthentication("CookieAuthentication")
    .AddCookie("CookieAuthentication", config =>
    {
        config.Cookie.Name = "UserLoginCookie";
        config.LoginPath = "/Conta/Login";
        config.ExpireTimeSpan = TimeSpan.FromDays(7);
        config.AccessDeniedPath = "/Conta/AcessoNegado";
    });

// =======================
// 🧱 MVC
// =======================
builder.Services.AddControllersWithViews();


var app = builder.Build();


// =======================
// ⚙️ Configuração do Pipeline
// =======================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 🌍 Ativar localização
app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

// 🔐 Autenticação e autorização
app.UseAuthentication();
app.UseAuthorization();

// =======================
// 🚀 Rotas
// =======================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
