using MedVoll.Web.Data;
using MedVoll.Web.Filters;
using MedVoll.Web.Interfaces;
using MedVoll.Web.Repositories;
using MedVoll.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ExceptionHandlerFilter>();

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<ExceptionHandlerFilter>();
});

// service de injeção de dependência
builder.Services.AddTransient<IMedicoRepository, MedicoRepository>();
builder.Services.AddTransient<IConsultaRepository, ConsultaRepository>();
builder.Services.AddTransient<IMedicoService, MedicoService>();
builder.Services.AddTransient<IConsultaService, ConsultaService>();

// Configuração do banco de dados SQLite
var connectionString = builder.Configuration.GetConnectionString("SqliteConnection");
builder.Services.AddDbContext<ApplicationDbContext>(x => x.UseSqlite(connectionString));

//  Adiciona o Identity com suporte a roles e EF Core
builder.Services
    .AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)  // exige confirmação de conta
    .AddRoles<IdentityRole>() // obrigatório para chamar o IdentitySeeders
    .AddEntityFrameworkStores<ApplicationDbContext>(); 


// Configuração do Identity para usar o SQLite 
builder.Services.Configure<IdentityOptions>(options =>
{
    options.SignIn.RequireConfirmedEmail = true; // Exigir e-mails confirmados para login
    options.SignIn.RequireConfirmedPhoneNumber = false; // Não exigir confirmação de número de telefone
});

// IdentityOptions - Configuração de bloqueio de conta
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Lockout.AllowedForNewUsers = true; // Permitir bloqueio para novos usuários
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(2); // Tempo de bloqueio após falhas
    options.Lockout.MaxFailedAccessAttempts = 2; // Máximo de tentativas de acesso falhadas antes do bloqueio
});

// IdentityOptions - Configuração de opções de senha
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true; // Exigir pelo menos um número
    options.Password.RequireLowercase = true; // Exigir pelo menos uma letra minúscula
    options.Password.RequireUppercase = true; // Exigir pelo menos uma letra maiúscula
    options.Password.RequireNonAlphanumeric = true; // Exigir caracteres especiais
    options.Password.RequiredLength = 8; // Tamanho mínimo da senha
});

// Configuração do cookie de autenticação
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login"; // Redireciona para login se não autenticado
    options.LogoutPath = "/Identity/Account/Logout"; // Caminho para logout
    options.AccessDeniedPath = "/Identity/Account/AccessDenied"; // Caminho para acesso negado
    options.ExpireTimeSpan = TimeSpan.FromMinutes(2); // Tempo de expiração
    options.SlidingExpiration = true; // Renova o cookie automaticamente

    options.Cookie.HttpOnly = true; // Impede acesso via JavaScript
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Exige HTTPS
    options.Cookie.SameSite = SameSiteMode.Strict; // Restringe envio de cookies entre sites
});

// Configuração do cookie de sessão
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.IdleTimeout = TimeSpan.FromMinutes(1);
});


// Configuração do Antiforgery Token 
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = "VollMed.AntiForgery"; // Nome personalizado do cookie
    options.Cookie.HttpOnly = true; // Evitar acesso via JavaScript
    options.HeaderName = "X-CSRF-TOKEN"; // Cabeçalho personalizado para APIs
});


builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSession();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/erro/500");
    app.UseStatusCodePagesWithReExecute("/erro/{0}");
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages().WithStaticAssets();


// Executa o IdentitySeeder para criar usuários e roles iniciais 
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await IdentitySeeder.SeedUsersAsync(services);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro ao executar o Seeder: {ex.Message}");
    }
}

// Middleware para adicionar cabeçalhos de segurança contra:
// 1. XSS (Cross-Site Scripting):
// 2. Sniffing de MIME Type:
app.Use(async (context, next) =>
{
    // Restringe fontes de conteúdo para evitar XSS.
    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; script-src 'self';");

    // Previne a interpretação incorreta de MIME types.
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    await next();
});

app.Run();
