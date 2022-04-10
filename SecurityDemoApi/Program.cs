using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<SecurityDemoDbContext>(options => 
    options.UseSqlite("Data Source=securitydemo.db"));

builder.Services.AddCors(options =>
    {
        options.AddPolicy("any", policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });

builder.Services.AddAuthentication("Bearer")
	.AddJwtBearer("Bearer", options =>
	{
		options.BackchannelHttpHandler = new HttpClientHandler { 
				ServerCertificateCustomValidationCallback = delegate { return true; } 
			};

		options.RequireHttpsMetadata = false;

		options.Authority = builder.Configuration.GetValue<string>("auth");

		options.Audience = "SecurityTestApi"; 

		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateAudience = false
		};
	});
    
builder.Services.AddAuthorization(options =>
	{
		options.AddPolicy("Protected", policy =>
		{
			policy.RequireAuthenticatedUser();
            policy.RequireClaim("scope", "SecurityTestScope");
		});
	});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("any");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
