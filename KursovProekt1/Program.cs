// Стартовият файл - настройва и стартира приложението
using ControlPanel.Data;
using ControlPanel.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Свързваме се с базата данни ──────────────────────────────────────────────
// Четем connection string-а от appsettings.json
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Показва подробни грешки при проблем с базата (само за разработка)
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ── Настройваме системата за потребители (Identity) ───────────────────────────
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    // Не изискваме потвърждение на имейл при регистрация
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddRoles<IdentityRole>()               // Добавяме поддръжка на роли
    .AddEntityFrameworkStores<ApplicationDbContext>(); // Пазим в нашата база

// Добавяме поддръжка за Controller + View
builder.Services.AddControllersWithViews();

// ── Настройки на бисквитката за вход ─────────────────────────────────────────
builder.Services.ConfigureApplicationCookie(options =>
{
    // При отказан достъп отиваме на тази страница
    options.AccessDeniedPath = "/Home/AccessDenied";

    // При всяка заявка проверяваме дали потребителят е активен
    options.Events.OnValidatePrincipal = async context =>
    {
        // Вземаме услугата за потребители
        var userManager = context.HttpContext.RequestServices
            .GetRequiredService<UserManager<ApplicationUser>>();

        // Намираме текущия потребител
        var потребител = await userManager.GetUserAsync(context.Principal);

        // Ако потребителят не съществува или е деактивиран - изхвърляме го
        bool трябваДаИзлезе = потребител == null || !потребител.IsActive;

        if (трябваДаИзлезе)
        {
            // Унищожаваме сесията
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync();

            // Записваме съобщение в бисквитка (1 минута)
            // За да го покажем на страницата за вход
            context.HttpContext.Response.Cookies.Append(
                "DeactivatedMessage",
                "Вашият акаунт е деактивиран. Свържете се с администратора.",
                new CookieOptions { MaxAge = TimeSpan.FromMinutes(1) }
            );
        }
    };
});

// ── Изграждаме приложението ───────────────────────────────────────────────────
var app = builder.Build();

// Изпълняваме началните данни (роли + admin акаунт)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedData.Initialize(services);
}

// ── Настройки за грешки ───────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    // В режим на разработка показваме подробни грешки
    app.UseMigrationsEndPoint();
}
else
{
    // В продукция показваме обща страница за грешка
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ── Тръбопровод на заявките (Middleware) ──────────────────────────────────────
// Редът е важен! Всяка заявка минава през тях по ред:

app.UseHttpsRedirection(); // Пренасочваме HTTP към HTTPS
app.UseStaticFiles();      // Позволяваме статични файлове (CSS, JS, снимки)
app.UseRouting();          // Определяме кой Controller ще отговори
app.UseAuthentication();   // Проверяваме кой е потребителят (вход)
app.UseAuthorization();    // Проверяваме какво може да прави потребителят

// ── Маршрутизация ─────────────────────────────────────────────────────────────
// По подразбиране: Home контролер, Index действие
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Маршрути за Identity страниците (вход, регистрация и т.н.)
app.MapRazorPages();

// Стартираме приложението
app.Run();