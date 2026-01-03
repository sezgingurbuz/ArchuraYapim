using basics.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// İki ayrı Cookie Authentication Scheme - Admin ve Müşteri için
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "CustomerScheme"; // Varsayılan müşteri
    options.DefaultChallengeScheme = "CustomerScheme";
})
.AddCookie("AdminScheme", options =>
{
    options.LoginPath = "/Admin/Login/Index";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.AccessDeniedPath = "/Admin/Login/AccessDenied";
    options.Cookie.Name = "AdminAuth";
})
.AddCookie("CustomerScheme", options =>
{
    options.LoginPath = "/Account/Login";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
    options.AccessDeniedPath = "/Account/Login";
    options.Cookie.Name = "CustomerAuth";
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    // Google ile giriş başarılı olduğunda CustomerScheme çerezine yazsın
    options.SignInScheme = "CustomerScheme";
});

// Authorization Policy'leri
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy => 
        policy.RequireAuthenticatedUser()
              .AddAuthenticationSchemes("AdminScheme")
              .RequireRole("Admin", "Editor", "Okan"));
    
    options.AddPolicy("CustomerPolicy", policy => 
        policy.RequireAuthenticatedUser()
              .AddAuthenticationSchemes("CustomerScheme"));
});


// Add DbContext with MySQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.Parse("8.0.0-mysql") // MySQL 8.0+
    ));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); //Önce Kimlik Kontrolü
app.UseAuthorization(); //Sonra Yetkilendirme


app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Admin}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
