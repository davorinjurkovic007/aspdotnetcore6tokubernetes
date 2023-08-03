using GloboTicket.Frontend;
using GloboTicket.Frontend.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddInfrastructureService();

builder.Services.AddHealthChecks()
    .AddCheck<SlowDependencyHealthCheck>("SlowDependencyDemo", tags: new string[] { "ready" })
    .AddProcessAllocatedMemoryHealthCheck(maximumMegabytesAllocated: 500);

builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Turning this off to simplify the running in Kubernetes demo
// app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=ConcertCatalog}/{action=Index}/{id?}");

//map the livelyness and readyness probes
app.MapHealthChecks("/health/ready",
new HealthCheckOptions()
{
    Predicate = reg => reg.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/lively",
new HealthCheckOptions()
{
    Predicate = reg => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});


app.Run();
