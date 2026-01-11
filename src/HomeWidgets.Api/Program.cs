using HomeWidgets.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Load local secrets file (gitignored) if it exists
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// Add services to the container
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Authentication & Authorization middleware (order matters!)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

