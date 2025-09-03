using FluentValidation;
using Stackbuld.ProductOrdering.Api.Filters;
using Stackbuld.ProductOrdering.Application;
using Stackbuld.ProductOrdering.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddValidatorsFromAssembly(typeof(Stackbuld.ProductOrdering.Application.DependencyInjection).Assembly);

builder.Services.AddScoped<GlobalExceptionFilter>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapHealthChecks("/health");

app.Run();
