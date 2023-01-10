using ConcordServicing.Web.Api;
using ConcordServicing.Web.Configuration;
using Foundatio.Extensions.Hosting.Startup;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddTransient<ClientAppEventDispatcher>();

builder.UseConcordWolverine();
builder.AddConcordDbContext();
builder.AddSampleDataStartupAction();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseWaitForStartupActionsBeforeServingRequests();

app.MapCustomerApi();

app.MapHub<ClientAppEventsHub>("/api/events");

app.MapFallbackToFile("index.html");

app.Run();
