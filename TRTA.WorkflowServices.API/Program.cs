using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.OpenApi.Models;
using Serilog;
using TRTA.OSP.Authentication.Service.DTO;
using TRTA.OSP.Authentication.Service.Handlers;
using TRTA.OSP.Authentication.Service.Interfaces;
using TRTA.OSP.Authentication.Service.Services;
using TRTA.WorkflowServices.ApplicationCore.AutoMapProfiles;
using TRTA.WorkflowServices.ApplicationCore.Services;
using TRTA.WorkflowServices.ExternalServices;
using TRTA.WorkflowServices.Repository.Repositories;

var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    var configuration = new ConfigurationBuilder()
                      .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                      .Build();

    ProcessCommandLineArguments(args, configuration);

    var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

    builder.Logging.AddSerilog(logger, dispose: true);


builder.Services.AddControllers();
//appsettings
builder.Services.Configure<JwtAuthentication>(builder.Configuration.GetSection("JwtAuthentication"));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAutoMapper(typeof(UsersProfile));

//configure services
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<ITokenService, TokenService>();

//configure repositories
builder.Services.AddTransient<IUserRepository, UserRepository>();

builder.Services.AddHttpClient<ILonestarSecurityService, LoneStarSecurityService>();
builder.Services.AddHttpClient<IUserAdminservice, UserAdminservice>();


builder.Services.AddAuthentication(auth =>
{
    auth.DefaultScheme = "Bearer";
}).AddScheme<JwtBearerOptions, JwtBearerAuthenticationHandler>("Bearer", null)
              .AddScheme<UdsAuthenticationOptions, UdsTokenAuthenticationHandler>("UDSLongToken", null);

var policy = new AuthorizationPolicyBuilder(new string[] { "Bearer", "UDSLongToken" }).RequireAuthenticatedUser().Build();
builder.Services.AddMvc().AddMvcOptions(options =>
{
    options.Filters.Add(new AuthorizeFilter(policy));
});
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "NZ Walks API", Version = "v1" });
    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = JwtBearerDefaults.AuthenticationScheme
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                }
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.MapControllers();

app.Run();

static void ProcessCommandLineArguments(string[] args, IConfigurationRoot configuration)
{
    try
    {
        //This will update the log file name based on commandline args passed for Instance.        
        configuration["Serilog:WriteTo:0:Args:path"] = configuration["Serilog:WriteTo:0:Args:path"].Replace("{ID}", args[0].Split("=")[1]);
        configuration["Serilog:WriteTo:0:Args:path"] = configuration["Serilog:WriteTo:0:Args:path"].Replace("{TYPE}", args[1].Split("=")[1]);
    }
    catch
    {
        //DO Nothing
    }
}

    

