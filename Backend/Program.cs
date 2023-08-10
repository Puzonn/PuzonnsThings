using PuzonnsThings.Hubs.WatchTogether;
using PuzonnsThings.Hubs.Wheel;
using PuzonnsThings.Hubs.Yahtzee;
using PuzonnsThings.Repositories;
using PuzonnsThings.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PuzonnsThings.Databases;
using System.Diagnostics;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMvc((options) =>
{
    var noContentFormatter = options.OutputFormatters.OfType<HttpNoContentOutputFormatter>().FirstOrDefault();
    if (noContentFormatter != null)
    {
        noContentFormatter.TreatNullValueAsNoContent = false;
    }
});

string? jwtKey = builder.Configuration["Jwt:Key"];
string? jwtAudience = builder.Configuration["Jwt:Audience"];
string? jwtIssuer = builder.Configuration["Jwt:Issuer"];

Debug.Assert(jwtKey != null);
Debug.Assert(jwtAudience != null);
Debug.Assert(jwtIssuer != null);

TokenValidationParameters ValidationParameters = new TokenValidationParameters()
{
    ValidateIssuer = false,
    ValidateAudience = false,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = jwtIssuer,
    ValidAudience = jwtIssuer,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
};

const string Origin = "AllowOrigin";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: Origin,
        policy =>
        {
            if (builder.Environment.IsDevelopment())
            {
                policy.WithOrigins("http://localhost:3000")
                .AllowCredentials()
                .AllowAnyHeader()
                .AllowAnyMethod();
            }
            else
            {
                policy.WithOrigins("https://puzonnsthings.pl")
                .AllowCredentials()
                .AllowAnyHeader()
                .AllowAnyMethod();
            }
        });
});

builder.Services.AddAuthentication(auth =>
{
    auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
                .AddJwtBearer(token =>
                {
                    token.RequireHttpsMetadata = false;
                    token.SaveToken = true;
                    token.TokenValidationParameters = ValidationParameters;
                    token.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var authToken = context.Request.Headers["Authorization"].ToString();

                            var token = !string.IsNullOrEmpty(accessToken) ? accessToken.ToString() :
                            !string.IsNullOrEmpty(authToken) ? authToken.Substring(7) : string.Empty;

                            var path = context.HttpContext.Request.Path;

                            if (!string.IsNullOrEmpty(token) && (path.StartsWithSegments("/services")))
                            {
                                context.Token = token;
                            }

                            return Task.CompletedTask;
                        }
                    };
                });


builder.Services.AddSignalR();

builder.Services.AddControllers();

builder.Services.AddDbContext<DatabaseContext>(x =>
{
    x.UseSqlite(builder.Configuration["ConnectionStrings:Db"]);
});



builder.Services.AddScoped<YahtzeeService>();
builder.Services.AddSingleton<WheelService>();
builder.Services.AddSingleton<MemoryLobbyCollector>();

builder.Services.AddScoped<WatchTogetherService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<LobbyRepository>();

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseWebSockets();

if (!builder.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors(Origin);
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<WatchTogetherHub>("/services/watchtogether");
app.MapHub<YahtzeeHub>("/services/yahtzeeservice");
app.MapHub<WheelHub>("/services/wheelservice");

app.Run();