using BusinessObject.DTO.Order;
using BusinessObject.DTO.Payment;
using DataAccess;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Repo.IRepository;
using Repo.Repository;
using Service.IService;
using Service.Service;
using Service.ToolRunner;
using System.Reflection;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
// ===== gọi DB =====
var connectionString = builder.Configuration.GetConnectionString("ServerConnection");
builder.Services.AddDbContext<HomeTrackDBContext>(options => options.UseSqlServer(connectionString));

// ===== check JWT Auth =====
var jwt = builder.Configuration.GetSection("Jwt");
var issuer = jwt["Issuer"];
var audience = jwt["Audience"];
var key = jwt["Key"];

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // dành cho DEV
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization();

// ===== Swagger (có Bearer auth + XML) =====
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "HomeTrack API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập token theo dạng: Bearer {token}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // XML comments (bật “Generate documentation file” trong project Properties)
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);

    // Chỉ include route bắt đầu bằng "api"
    c.DocInclusionPredicate((doc, api) => api.RelativePath != null && api.RelativePath.StartsWith("api"));
});

// ===== Controllers =====
builder.Services.AddControllers();

// ===== CORS (dev) =====
var MyCors = "AllowLocal";
builder.Services.AddCors(opt =>
{
    opt.AddPolicy(MyCors, p => p
        .WithOrigins(
            "https://localhost:7227", // swagger cùng app
            "http://localhost:5173",  // vite
            "http://localhost:3000"   // react
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
    );
});

// ===== DI =====
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IWebhookLogRepository, WebhookLogRepository>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IPlanPriceRepository, PlanPriceRepository>();
builder.Services.AddScoped<IPlanPriceService, PlanPriceService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionAuditService>();

builder.Services.AddHostedService<MidnightRunner>();


builder.Services.Configure<PayOSOptions>(builder.Configuration.GetSection("PayOS"));



var app = builder.Build();
app.UseDeveloperExceptionPage();
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(MyCors);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Endpoint test nhanh
app.MapGet("/", () => Results.Ok("Hometrack API running"));
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
