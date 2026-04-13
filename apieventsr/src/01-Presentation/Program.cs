using apieventsr.Api;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.ConfigureServices(builder.Configuration, builder.Environment);

// Build app
var app = builder.Build();

// Configure App
app.Configure();

// Run App
app.Run();
