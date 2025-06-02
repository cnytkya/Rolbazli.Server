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
    .AddIdentity<AppUser,IdentityRole>() //Uygulamada AppUser (IdentityUser'dan t�reyen kullan�c�) ve IdentityRole s�n�flar�na g�re bir kimlik sistemi olu�turulur. Kullan�c� giri�i, ��k���, kay�t, �ifre i�lemleri vb. bu sistem �zerinden yap�lacak �ekilde ayarlan�r. Yani burda kimlik sistemi kurulur.
    .AddEntityFrameworkStores<AppDbContext>() //Kullan�c� ve rol bilgilerini Entity Framework Core kullanarak AppDbContext �zerinden veritaban�nda sakla.Yani Veriler EF Core ile AppDbContext �zerinden saklan�r. 
    .AddDefaultTokenProviders(); //AddDefaultTokenProviders() metodu �a�r�larak �ifre s�f�rlama, email onay� gibi i�lemlerde kullan�lacak token �retici servisler sisteme eklenir. �r; �ifre s�f�rlama token��.
      //�zet; kullan�c� (AppUser) ve rol (IdentityRole) y�netimi i�in ASP.NET Core Identity sistemini yap�land�r�r ve kimlik verilerini AppDbContext �zerinden Entity Framework Core ile veritaban�nda saklamas�n� sa�lar. Bu yap� ile birlikte bir kullan�c� register/login sistemi kurulur.

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
// Swagger i�in servisleri ekliyoruz
builder.Services.AddSwaggerGen(x =>
{
    // JWT token ile g�venli�e dair Swagger UI'de bir tan�m ekliyoruz
    x.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        // A��klama: Swagger UI'de g�z�kecek a��klama metni.
        // Kullan�c�ya token nas�l girilir �rne�i veriliyor.
        Description = @"JWT Authorization Example : `Bearer �ljdjsodsadh�sh�o`",

        // HTTP header �zerinden token g�nderilece�ini belirtiyoruz.
        Name = "Authorization",

        // Token'�n nerede yer alaca��n� belirtiyoruz. Burada Header (ba�l�k k�sm�nda) olacak.
        In = ParameterLocation.Header,

        // G�venlik �emas� tipi olarak "ApiKey" kullan�yoruz.
        // Swagger i�in JWT'de genelde bu �ekilde tan�mlan�r.
        Type = SecuritySchemeType.ApiKey,

        // �ema ismi "Bearer" olarak belirtiliyor.
        // Bu, Authorization header'�nda "Bearer <token>" �eklinde bir kullan�m olaca��n� ifade eder.
        Scheme = "Bearer"
    });
    // Swagger'a g�venlik gereksinimi ekliyoruz
    x.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {   
            // Burada "Bearer" �emas�na referans veriyoruz
            new OpenApiSecurityScheme
            {
                // Swagger'a, daha �nce tan�mlad���m�z "Bearer" g�venlik tan�m�n� referans almas�n� s�yl�yoruz.
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme, // Bu bir g�venlik �emas� referans�d�r
                    Id = "Bearer"// Daha �nce "AddSecurityDefinition" i�inde verdi�imiz isimle ayn� olmal�.
                },
                Scheme = "Bearer",// Header'da kullan�lan isim, yani "Authorization"
                Name = "Bearer",
                In = ParameterLocation.Header, // Token'�n g�nderilece�i yer: Header
            },
             // Bu �ema i�in gerekli yetki (scope) listesi.
            // JWT'de genellikle bo� b�rak�l�r ��nk� yetkilendirme backend'de yap�l�r.
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
