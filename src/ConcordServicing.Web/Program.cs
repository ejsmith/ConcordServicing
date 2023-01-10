using ConcordServicing.Web.Api;
using ConcordServicing.Web.Configuration;
using Foundatio.Extensions.Hosting.Startup;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});

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

app.MapFallbackToFile("index.html");

app.Run();
