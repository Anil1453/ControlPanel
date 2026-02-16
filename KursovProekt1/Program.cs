using KursovProekt1.Data;
using KursovProekt1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

//svurzva programata s sql'a (o tova se izvurshva v appsettings.json)
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Pomaga za debug na greshki svurzani s bazata danni (samo za development)
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//vkluchva sistemata za potrebiteli AddDefaultIdentity AddRoles AddEntityFrameworkStores
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// aktivira MVC'to v stranicata
builder.Services.AddControllersWithViews();

var app = builder.Build();

// SUZDAVAME ROLI I ADMIN PRI STARTIRANE NA PROGRAMATA
using (var scope = app.Services.CreateScope())
{
    // Vzimame services (uslugite) ot scope
    var services = scope.ServiceProvider;

    // Izvikvame SeedData.Initialize za da suzdadem roli i admin
    await SeedData.Initialize(services);
}

// Ako sme v development rezhim - pokaji specialna stranica za greshki s migration
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

//Prenasochva HTTP -> HTTPS sigurna vruzka
app.UseHttpsRedirection();

//Pozvolqva na izpolzvane css,JS, i kartinki i t.n
app.UseStaticFiles();

// Routing - marshruti
app.UseRouting();

// Proverqva KOI e potrebitelqt (dali e vlqzul v sistema)
app.UseAuthentication();

// proverqva dali potrebitelq ima poravo da vijda stranicata
app.UseAuthorization();

//Задава URL структурата.
//На прост език:
//yourdomain.com / → отива към HomeController → Index метод
//yourdomain.com/Room/List → отива към RoomController → List метод
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//Pokazva Identity strainicite (login,register)
app.MapRazorPages();

app.Run();