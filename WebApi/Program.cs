using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebApi.Data;
using WebApi.GraphQL;
using WebApi.GraphQL.Mutations;

var builder = WebApplication.CreateBuilder(args);

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
        .EnableDetailedErrors()
        .EnableSensitiveDataLogging()
);

var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<JwtOptions>(jwtSection);

var key = Encoding.UTF8.GetBytes(jwtSection["Key"]);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // true en prod con HTTPS
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// --- SERVICIOS DE HOT CHOCOLATE ---
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()       // Registra 'Query.cs'
    .AddMutationType<Mutation>()
    .AddTypeExtension<EmployeeMutations>()
    .AddTypeExtension<BrandMutations>()  
    .AddTypeExtension<ActiveIngredientMutations>()
    .AddTypeExtension<AdministrationRouteMutations>()
    .AddTypeExtension<CategoryMutations>()
    .AddTypeExtension<ManufacturerMutations>()
    .AddTypeExtension<PresentationMutations>()
    .AddTypeExtension<UnitOfMeasureMutations>() 
    .AddTypeExtension<MedicineMutations>()
    .AddTypeExtension<BatchMutations>()
    .AddTypeExtension<PromotionMutations>()
    .AddTypeExtension<PaymentMethodMutations>()
    .AddTypeExtension<SaleMutations>()
    .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = true)
    .AddProjections()            // Habilita [UseProjection]
    .AddFiltering()              // Habilita [UseFiltering]
    .AddSorting()               // Habilita [UseSorting]
    .AddCostAnalyzer() 
    .ModifyCostOptions(o => o.MaxFieldCost = 7000);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

// --- MAPEAR EL ENDPOINT DE GRAPHQL ---
app.MapGraphQL(); // Esto crea la URL /graphql

app.Run();

public class JwtOptions
{
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public string Key { get; set; }
    public int ExpireMinutes { get; set; }
}
