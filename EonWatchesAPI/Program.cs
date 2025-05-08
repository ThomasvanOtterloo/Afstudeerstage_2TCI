using EonWatchesAPI.DbContext;
using EonWatchesAPI.DbContext.I_Repositories;
using EonWatchesAPI.Factories.Notifications;
using EonWatchesAPI.Services.I_Services;
using EonWatchesAPI.Services.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// 1) Register a CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
    {
        policy
           .AllowAnyOrigin()
           .AllowAnyHeader()
           .AllowAnyMethod();
    });
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.Local.json", optional: false, reloadOnChange: true);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true; // Optional: for better readability
    });

builder.Services.Configure<GmailSettings>(
    builder.Configuration.GetSection(GmailSettings.GmailOptionsKey));



var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.Configure<GmailSettings>(
    builder.Configuration.GetSection(GmailSettings.GmailOptionsKey));

// background services
builder.Services.AddHostedService<TriggerCheckerService>();


// DB repos
builder.Services.AddScoped<IAdRepository, AdRepository>();
builder.Services.AddScoped<ITraderRepository, TraderRepository>();
builder.Services.AddScoped<IDistributeAdRepository, DistributeAdRepository>();
builder.Services.AddScoped<ITriggerRepository, TriggerRepository>();

// services
builder.Services.AddScoped<IAdService, AdService>();
builder.Services.AddScoped<ITraderService, TraderService>();
builder.Services.AddScoped<IDistributeAdService, DistributeAdService>();
builder.Services.AddScoped<INotification, MailNotification>();
builder.Services.AddScoped<ITriggerService, TriggerService>();


builder.Logging
       .ClearProviders()
       .AddConsole()
       .AddDebug();



var app = builder.Build();
app.UseCors("AllowAngularDev");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();
app.MapControllers();
app.Run();