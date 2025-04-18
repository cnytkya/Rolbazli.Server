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
var JWTSetting = builder.Configuration.GetSection("JWTSetting");//Uygulaman�n konfigurasyonundan JWT ayarlar�n� al�yoruz.

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddIdentity<AppUser, IdentityRole>()//Uygulama AppUser (IdentityUser'dan t�reyen kullan�c�) ve IdentityRole s�n�flar�na g�re bir kimlik sistemi olu�turulur. Kullan�c� giri�i, ��k���, kay�t, �ifre i�lemleri vb. bu sistem �zerinden yap�lacak �ekilde ayarlan�r. Sonu� olarak burda bir kimlik sistemi kurulur.
    .AddEntityFrameworkStores<AppDbContext>()//Kullan�c� ve rol bilgilerini EF Core kullanarak AppDbContext �zerinden veritaban�nda sakla. 
    .AddDefaultTokenProviders(); //AddDefaultTokenProviders() metodu �a�r�larak �ifre yenileme, email onay� gibi i�lemlerde kullan�lack token �reticisi servisler sisteme eklenir. �r: �ifre s�f�rlama token'�.
/*
        --------------------�zet-----------------------
        Kullan�c� (AppUser) ve rol (IdentityRole) y�netimi i�in asp.net core Identity sistemini yap�land�r�r ve kimlik verilerini AppDbContext �zerinden ef core ile veritaban�nda saklanmas�n� sa�lar. Bu yap� ile birlikte bir kullan�c� register/login sistemi kurulur.
*/

// JWT tabanl� kimlik do�rulamay� servislere ekleme
builder.Services.AddAuthentication(opt =>
{
    //Varsay�lan kimlik do�rulama �emas� olarak JWT Bearer kullan�m�
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    //Kimlik do�rulama ba�ar�s�z ise kullan�lacak �ema
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    //Uygulamada kullan�lacak varsay�lan �ema
    opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(opt =>//JWT Bearer yap�land�rmas�
  {
      //Token'� do�rulad�ktan sonra saklya�p va�ka yerlerde kullanabilmek i�in kayedediyoruz.
      opt.SaveToken = true;
      //HTTPS zorunlu olmas�n
      opt.RequireHttpsMetadata = false;

      //Token do�rulama parametrelerini ayarlama
      opt.TokenValidationParameters = new TokenValidationParameters
      {
          //Token'�n "issuer" (veren) bilgisini do�rula
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidateLifetime = true,
          ValidateIssuerSigningKey = true,
          ValidAudience = JWTSetting["ValidAudience"],
          ValidIssuer = JWTSetting["ValidIssuer"],
          //Token'� do�rulamak i�in kullan�lacak imza anahtar�(gizli anahtar)
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
        //Swagger UI'de g�z�kecek a��klama metni.
        Description = @"JWT Authorization Example : `Bearer adsksjdhasdhhasbdbj`",
        Name = "Authorization",
        //Token'�n nerede yer alaca��n� belirtelim. Header k�sm�nda olsun.
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey, //G�venlik �emas� tipi
        //�ema ismi "Bearer" olarak tan�mla.
        Scheme = "Bearer"
    });
    //swagger'a g�venlik gereksinimleri ekleyece�iz.
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
