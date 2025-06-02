using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Rolbazli.Data;
using Rolbazli.Model.Models;

var builder = WebApplication.CreateBuilder(args);
var JWTSetting = builder.Configuration.GetSection("JWTSetting");
// Add services to the container.

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddIdentity<AppUser,IdentityRole>() //Uygulamada AppUser (IdentityUser'dan türeyen kullanýcý) ve IdentityRole sýnýflarýna göre bir kimlik sistemi oluþturulur. Kullanýcý giriþi, çýkýþý, kayýt, þifre iþlemleri vb. bu sistem üzerinden yapýlacak þekilde ayarlanýr. Yani burda kimlik sistemi kurulur.
    .AddEntityFrameworkStores<AppDbContext>() //Kullanýcý ve rol bilgilerini Entity Framework Core kullanarak AppDbContext üzerinden veritabanýnda sakla.Yani Veriler EF Core ile AppDbContext üzerinden saklanýr. 
    .AddDefaultTokenProviders(); //AddDefaultTokenProviders() metodu çaðrýlarak þifre sýfýrlama, email onayý gibi iþlemlerde kullanýlacak token üretici servisler sisteme eklenir. Ör; þifre sýfýrlama token’ý.
      //Özet; kullanýcý (AppUser) ve rol (IdentityRole) yönetimi için ASP.NET Core Identity sistemini yapýlandýrýr ve kimlik verilerini AppDbContext üzerinden Entity Framework Core ile veritabanýnda saklamasýný saðlar. Bu yapý ile birlikte bir kullanýcý register/login sistemi kurulur.

builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(opt =>
{
    opt.SaveToken = true;
    opt.RequireHttpsMetadata = false;
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidAudience = JWTSetting["ValidAudience"],
        ValidIssuer = JWTSetting["ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JWTSetting.GetSection("securityKey").Value!))
    };
});

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
// Swagger için servisleri ekliyoruz
builder.Services.AddSwaggerGen(x =>
{
    // JWT token ile güvenliðe dair Swagger UI'de bir taným ekliyoruz
    x.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        // Açýklama: Swagger UI'de gözükecek açýklama metni.
        // Kullanýcýya token nasýl girilir örneði veriliyor.
        Description = @"JWT Authorization Example : `Bearer þljdjsodsadhýshýo`",

        // HTTP header üzerinden token gönderileceðini belirtiyoruz.
        Name = "Authorization",

        // Token'ýn nerede yer alacaðýný belirtiyoruz. Burada Header (baþlýk kýsmýnda) olacak.
        In = ParameterLocation.Header,

        // Güvenlik þemasý tipi olarak "ApiKey" kullanýyoruz.
        // Swagger için JWT'de genelde bu þekilde tanýmlanýr.
        Type = SecuritySchemeType.ApiKey,

        // Þema ismi "Bearer" olarak belirtiliyor.
        // Bu, Authorization header'ýnda "Bearer <token>" þeklinde bir kullaným olacaðýný ifade eder.
        Scheme = "Bearer"
    });
    // Swagger'a güvenlik gereksinimi ekliyoruz
    x.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {   
            // Burada "Bearer" þemasýna referans veriyoruz
            new OpenApiSecurityScheme
            {
                // Swagger'a, daha önce tanýmladýðýmýz "Bearer" güvenlik tanýmýný referans almasýný söylüyoruz.
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme, // Bu bir güvenlik þemasý referansýdýr
                    Id = "Bearer"// Daha önce "AddSecurityDefinition" içinde verdiðimiz isimle ayný olmalý.
                },
                Scheme = "Bearer",// Header'da kullanýlan isim, yani "Authorization"
                Name = "Bearer",
                In = ParameterLocation.Header, // Token'ýn gönderileceði yer: Header
            },
             // Bu þema için gerekli yetki (scope) listesi.
            // JWT'de genellikle boþ býrakýlýr çünkü yetkilendirme backend'de yapýlýr.
            new List<string>()
        }
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(opt =>
{
    opt.AllowAnyHeader();
    opt.AllowAnyMethod();
    opt.AllowAnyOrigin();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
