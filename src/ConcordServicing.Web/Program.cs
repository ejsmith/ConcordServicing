using ConcordServicing.Data;
using ConcordServicing.Web.Api;
using ConcordServicing.Web.Configuration;
using Foundatio.Extensions.Hosting.Startup;
using Oakton;
using OpenTelemetry;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddTransient<ClientAppEventDispatcher>();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks().AddDbContextCheck<ConcordDbContext>(); ;

builder.UseConcordWolverine();
builder.AddConcordDbContext();
builder.AddOpenTelemetry();
builder.AddCreateSampleDataStartupAction();

var app = builder.Build();

// ensure the database is created
if (app.Environment.IsDevelopment())
    await app.Services.GetRequiredService<ConcordDbContext>().Database.EnsureCreatedAsync();

app.UseSwagger();
app.UseSwaggerUI(o => o.InjectStylesheet("/swagger/docs.css"));
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseWaitForStartupActionsBeforeServingRequests();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app.UseStatusCodePages();

app.MapCustomerApi();

app.MapGet("/api/exception", () => { throw new InvalidOperationException("Sample Exception"); });

app.MapHub<ClientAppEventsHub>("/api/events");

app.MapHealthChecksWithJsonResponse("/healthz");
app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.MapFallbackToFile("index.html");

await app.RunOaktonCommands(args);
