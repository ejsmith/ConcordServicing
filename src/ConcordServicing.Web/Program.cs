using ConcordServicing.Data;
using ConcordServicing.Web.Api;
using ConcordServicing.Web.Configuration;
using Foundatio.Extensions.Hosting.Startup;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddTransient<ClientAppEventDispatcher>();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks().AddDbContextCheck<ConcordDbContext>(); ;

builder.UseConcordWolverine();
builder.AddConcordDbContext();
builder.AddSampleDataStartupAction();

var app = builder.Build();

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

app.MapHealthChecks("/healthz");
app.MapFallbackToFile("index.html");

app.Run();
