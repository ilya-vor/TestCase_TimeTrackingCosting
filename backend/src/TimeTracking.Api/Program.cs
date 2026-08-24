using TimeTracking.Api;
using TimeTracking.Application.Common;
using TimeTracking.Application;
using TimeTracking.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var mongoConnection = builder.Configuration.GetConnectionString("Mongo")
    ?? "mongodb://localhost:27017/?replicaSet=rs0";
var mongoDatabase = builder.Configuration["Mongo:Database"] ?? "time_tracking";

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .Where(m => !string.IsNullOrWhiteSpace(m));
            var message = string.Join("; ", errors);
            if (string.IsNullOrWhiteSpace(message))
                message = "Некорректные параметры запроса.";

            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(new { code = ErrorCodes.ValidationError, message });
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(mongoConnection, mongoDatabase);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
