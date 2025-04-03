using EonWatchesAPI.DbContext;
using EonWatchesAPI.DbContext.I_Repositories;
using EonWatchesAPI.Services.I_Services;
using EonWatchesAPI.Services.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));


// DB repos
builder.Services.AddScoped<IAdRepository, AdRepository>();
builder.Services.AddScoped<ITraderRepository, TraderRepository>();
builder.Services.AddScoped<IDistributeAdRepository, DistributeAdRepository>();
builder.Services.AddScoped<ITriggerRepository, TriggerRepository>();


// services
builder.Services.AddScoped<IAdService, AdService>();
builder.Services.AddScoped<ITraderService, TraderService>();
builder.Services.AddScoped<IDistributeAdService, DistributeAdService>();
builder.Services.AddScoped<ITriggerService, TriggerService>();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();




app.Run();
