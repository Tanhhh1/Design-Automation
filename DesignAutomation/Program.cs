using Autodesk.Forge.Core;
using Autodesk.Forge.DesignAutomation;
using DesignAutomation.Config;
using DesignAutomation.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var apsSection = builder.Configuration.GetSection("APS");
builder.Services.Configure<APSOptions>(apsSection);
builder.Services.Configure<ForgeConfiguration>(config =>
{
    config.ClientId = apsSection["ClientId"];
    config.ClientSecret = apsSection["ClientSecret"];
});

builder.Services.AddDesignAutomation(apsSection);

builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AppBundleService>();
builder.Services.AddScoped<ActivityService>();
builder.Services.AddScoped<WorkItemService>();
builder.Services.AddScoped<OssService>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();