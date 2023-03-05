using API;
using API.Controllers;
using Auth0;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

var appSettings = builder.Configuration.GetSection("AppSettings");
builder.Services.Configure<AppSettings>(appSettings);

// ENABLE JWT USAGE *************************************//

//IdentityModelEventSource.ShowPII = true;

var _allowedOrigins = new List<string>()
{
    "https://localhost:7088"
};

builder.Services.AddCors(options =>
{
    options.AddPolicy("APP-POLICY",
        policy =>
        {
            policy
                .WithOrigins(_allowedOrigins.ToArray())
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});


builder.Services.AddAuthentication()
    .AddJwtBearer("MANAGEMENT", options => //MANAGEMENT
{
    options.Authority = appSettings.GetValue<string>("ClientAPI:Domain");
    options.Audience = "https://dev-bqyr08coyzznixfx.eu.auth0.com/api/v2/";
    //options.RequireHttpsMetadata = false;
}).AddJwtBearer("WEBAPI", options => //Web API
{
    options.Authority = appSettings.GetValue<string>("ClientAPI:Domain");
    options.Audience = appSettings.GetValue<string>("ClientAPI:Audience");
});


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("create:users", policy => policy.AddAuthenticationSchemes("MANAGEMENT").Requirements.Add(new HasScopeRequirement("create:users", appSettings.GetValue<string>("ClientAPI:Domain"))));
    options.AddPolicy("read:messages", policy => policy.AddAuthenticationSchemes("WEBAPI").Requirements.Add(new HasScopeRequirement("read:messages", appSettings.GetValue<string>("ClientAPI:Domain"))));

    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .AddAuthenticationSchemes("MANAGEMENT", "WEBAPI")
        .Build();
});

// register the scope authorization handler
builder.Services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Auth Example API",
        Version = "v1",
        Description = ""
    });
    options.OperationFilter<AddResponseHeadersFilter>();
    options.OperationFilter<SecurityRequirementsOperationFilter>();
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Standard Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
});

builder.Services.AddHttpClient<ManagementController>();
builder.Services.AddHttpClient<UserController>();

var app = builder.Build();

app.UseCors("APP-POLICY");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
