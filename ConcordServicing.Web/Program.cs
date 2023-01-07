using ConcordServicing.Web.Api;
using ConcordServicing.Web.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.UseConcordWolverine();
builder.AddConcordDbContext();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.MapCustomerApi();

app.Run();
