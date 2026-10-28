using Microsoft.EntityFrameworkCore;
using AemenersolSync.Api.Data;
using AemenersolSync.Api.Hubs;
using AemenersolSync.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddHttpClient<IAemenersolClient, AemenersolClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Aemenersol:BaseUrl"]!);
});

builder.Services.AddScoped<IPlatformWellRepository, PlatformWellRepository>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});
builder.Services.AddSignalR();
builder.Services.AddScoped<ISyncTokenStore, SessionSyncTokenStore>();
builder.Services.AddScoped<ISyncService, SyncService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseSession();
app.UseCors("Frontend");
app.MapHub<SyncHub>("/hubs/sync");

app.MapControllers();

app.Run();
