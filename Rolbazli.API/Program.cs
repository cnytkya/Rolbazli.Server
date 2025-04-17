using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Rolbazli.Data;
using Rolbazli.Model.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var JWTSetting = builder.Configuration.GetSection("JWTSetting");//Uygulamanýn konfigurasyonundan JWT ayarlarýný alýyoruz.

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddIdentity<AppUser, IdentityRole>()//Uygulama AppUser (IdentityUser'dan türeyen kullanýcý) ve IdentityRole sýnýflarýna göre bir kimlik sistemi oluþturulur. Kullanýcý giriþi, çýkýþý, kayýt, þifre iþlemleri vb. bu sistem üzerinden yapýlacak þekilde ayarlanýr. Sonuç olarak burda bir kimlik sistemi kurulur.
    .AddEntityFrameworkStores<AppDbContext>()//Kullanýcý ve rol bilgilerini EF Core kullanarak AppDbContext üzerinden veritabanýnda sakla. 
    .AddDefaultTokenProviders(); //AddDefaultTokenProviders() metodu çaðrýlarak þifre yenileme, email onayý gibi iþlemlerde kullanýlack token üreticisi servisler sisteme eklenir. Ör: þifre sýfýrlama token'ý.
/*
        --------------------özet-----------------------
        Kullanýcý (AppUser) ve rol (IdentityRole) yönetimi için asp.net core Identity sistemini yapýlandýrýr ve kimlik verilerini AppDbContext üzerinden ef core ile veritabanýnda saklanmasýný saðlar. Bu yapý ile birlikte bir kullanýcý register/login sistemi kurulur.
*/

// JWT tabanlý kimlik doðrulamayý servislere ekleme
builder.Services.AddAuthentication(opt =>
{
    //Varsayýlan kimlik doðrulama þemasý olarak JWT Bearer kullanýmý
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    //Kimlik doðrulama baþarýsýz ise kullanýlacak þema
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    //Uygulamada kullanýlacak varsayýlan þema
    opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(opt =>//JWT Bearer yapýlandýrmasý
  {
      //Token'ý doðruladýktan sonra saklyaýp vaþka yerlerde kullanabilmek için kayedediyoruz.
      opt.SaveToken = true;
      //HTTPS zorunlu olmasýn
      opt.RequireHttpsMetadata = false;

      //Token doðrulama parametrelerini ayarlama
      opt.TokenValidationParameters = new TokenValidationParameters
      {
          //Token'ýn "issuer" (veren) bilgisini doðrula
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidateLifetime = true,
          ValidateIssuerSigningKey = true,
          ValidAudience = JWTSetting["ValidAudience"],
          ValidIssuer = JWTSetting["ValidIssuer"],
          //Token'ý doðrulamak için kullanýlacak imza anahtarý(gizli anahtar)
          IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(JWTSetting.GetSection("secretKey").Value!))
      };
});



builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(x =>
{
    x.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        //Swagger UI'de gözükecek açýklama metni.
        Description = @"JWT Authorization Example : `Bearer adsksjdhasdhhasbdbj`",
        Name = "Authorization",
        //Token'ýn nerede yer alacaðýný belirtelim. Header kýsmýnda olsun.
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey, //Güvenlik þemasý tipi
        //þema ismi "Bearer" olarak tanýmla.
        Scheme = "Bearer"
    });
    //swagger'a güvenlik gereksinimleri ekleyeceðiz.
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

app.UseAuthorization();

app.MapControllers();

app.Run();
