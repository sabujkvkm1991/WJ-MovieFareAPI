using Polly;
using System.Net;
using MovieFare.Application.Settings;
using MovieFare.Middlewares;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Polly.Extensions.Http;
using Polly.Timeout;
using MovieFare.Infrastructure.Services;
using MovieFare.Infrastructure.Settings;
using MovieFare.Application.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAngularLocalhost", policy =>
	{
		policy.WithOrigins("http://localhost:4200") 
			  .AllowAnyHeader()
			  .AllowAnyMethod();
	});
});

// Add services to the container.
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.RequireHttpsMetadata = true;
	options.SaveToken = true;
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidIssuer = jwtSettings.Issuer,
		ValidateAudience = true,
		ValidAudience = jwtSettings.Audience,
		ValidateIssuerSigningKey = true,
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
		ValidateLifetime = true
	};
});

builder.Services.AddAuthorization();

builder.Services.AddControllers();


var logger = new LoggerConfiguration()
	.WriteTo.File(new Serilog.Formatting.Json.JsonFormatter(),
				  @"C:\Logs\MovieFare.json",
				  rollingInterval: RollingInterval.Hour)
	.Enrich.FromLogContext()
	.CreateLogger();

Log.Logger = logger;

var movieSettings = builder.Configuration.GetSection("MovieSettings").Get<MovieSettings>();
if (movieSettings == null)
{
	throw new InvalidOperationException("MovieSettings configuration is missing or invalid.");
}
builder.Services.AddSingleton(movieSettings);

builder.Services.AddMediatR(cfg =>
	cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddHttpClient<IMovieService, MovieService>()
	.AddPolicyHandler(HttpPolicyExtensions
				.HandleTransientHttpError()
				.Or<TimeoutRejectedException>() 
				.WaitAndRetryAsync(
					retryCount: 3,
					sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
					onRetry: (outcome, timespan, retryAttempt, context) =>
					{
						Log.Logger.Information($"Retry {retryAttempt} after {timespan.TotalSeconds}s due to: {outcome.Exception?.Message}");
					}))
	.AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(
				TimeSpan.FromSeconds(10), TimeoutStrategy.Pessimistic));

builder.Services.AddScoped<IMovieService, MovieService>(); 
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddMemoryCache();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
	{
		Name = "Authorization",
		Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
		Scheme = "Bearer",
		BearerFormat = "JWT",
		In = Microsoft.OpenApi.Models.ParameterLocation.Header,
		Description = "Enter 'Bearer' followed by your JWT token.\r\n\r\nExample: `Bearer abc123def456`"
	});

	options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
	{
		{
			new Microsoft.OpenApi.Models.OpenApiSecurityScheme
			{
				Reference = new Microsoft.OpenApi.Models.OpenApiReference
				{
					Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
					Id = "Bearer"
				}
			},
			new string[] {}
		}
	});
});

builder.Host.UseSerilog();

var app = builder.Build();

app.UseCors("AllowAngularLocalhost");
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
