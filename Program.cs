// Program.cs
using System.Text; // Para Encoding
using LavandariaGaivotaAPI.Data;
using LavandariaGaivotaAPI.Models; // Adicione se criou ApplicationUser
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar o DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Para SQLite, a connection string é algo como "DataSource=app.db"
// Vamos adicioná-la ao appsettings.json mais tarde
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));// Fallback para o nome do ficheiro da BD

// 2. Configurar o ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => // Use ApplicationUser se o criou
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false; // Pode tornar mais restrito
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders(); // Para tokens de reset de password, etc.

// 3. Configurar a Autenticação JWT
// Obtenha as configurações do JWT do appsettings.json (vamos criar esta secção)
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var validAudiences = jwtSettings["ValidAudience"]?
    .Split(',', StringSplitOptions.RemoveEmptyEntries)
    .Select(a => a.Trim())
    .ToArray();
var secretKey = jwtSettings["SecretKey"]; // Chave secreta para assinar o token

if (string.IsNullOrEmpty(secretKey))
{
    secretKey = "T6J48gwWRkuVsfZ0Sn53M8vflbikvKdb";
    if (secretKey.Length < 32) // Exemplo de verificação de comprimento mínimo para certas chaves HMAC
    {
        throw new ArgumentException("A chave secreta do JWT deve ter pelo menos 32 bytes (256 bits) para algoritmos como HS256.");
    }
    Console.WriteLine("AVISO: Chave secreta JWT não configurada em appsettings.json. Usando chave de desenvolvimento padrão.");
}


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; // Em produção, defina para true
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["ValidIssuer"] ?? "http://localhost:5236", // Ex: http://localhost:5000
        //ValidAudience = jwtSettings["ValidAudience"] ?? "http://localhost:3000", // Ex: http://localhost:3000
        ValidAudiences = validAudiences ?? ["http://localhost:3000"], // Ex: http://localhost:3000
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Adicionar um título e versão à sua API no Swagger (boa prática)
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Lavandaria Gaivota API", Version = "v1" });

    // Definir o esquema de segurança para JWT Bearer Token
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Por favor, insira 'Bearer ' seguido do seu token JWT", // Instruções para o utilizador
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Adicionar um requisito de segurança global para usar o esquema "Bearer"
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
}); // Swagger é útil para testar a API

// Configurar CORS para permitir pedidos do seu frontend React
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policyBuilder =>
        {
            policyBuilder.WithOrigins(validAudiences ?? ["http://localhost:3000"]) // A porta do seu frontend React
                         .AllowAnyHeader()
                         .AllowAnyMethod();
        });
});

builder.WebHost.UseUrls("http://localhost:5236", "http://0.0.0.0:5236");
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // Aplicar migrations automaticamente em desenvolvimento (opcional, mas útil)
    // using (var scope = app.Services.CreateScope())
    // {
    //     var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    //     dbContext.Database.Migrate();
    // }
}

// app.UseHttpsRedirection(); // Comentado porque usámos --no-https. Se adicionar HTTPS, descomente.

app.UseCors("AllowReactApp"); // Aplicar a política CORS

app.UseAuthentication(); // IMPORTANTE: Adicionar antes de UseAuthorization
app.UseAuthorization();

app.MapControllers();


try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var configuration = services.GetRequiredService<IConfiguration>();
        await SeedData.Initialize(services, configuration);
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Ocorreu um erro durante o seeding da base de dados.");
}

app.Run();