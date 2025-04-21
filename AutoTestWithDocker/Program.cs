using AutoTestWithDocker;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddOptions<EnvSettings>()
    .Bind(builder.Configuration.GetSection(EnvSettings.ConfigurationSection))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddTransient<SomeService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecasts
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return Results.Ok(forecast);
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.MapGet("async-void", (int start, int end, [FromServices] SomeService service, CancellationToken cancellationToken) =>
{
    var result = service.GetDataWithAsyncVoid(start, end, cancellationToken);
    return Task.FromResult(Results.Ok(result));
});

app.MapGet("async-task", (int start, int end, [FromServices] SomeService service, CancellationToken cancellationToken) =>
{
    var result = service.GetDataWithAsyncTask(start, end, cancellationToken);
    return Task.FromResult(Results.Ok(result));
});

app.MapGet("async-task-catch-in-run", (int start, int end, [FromServices] SomeService service, CancellationToken cancellationToken) =>
{
    var result = service.GetDataWithAsyncTaskTryCatchInRun(start, end, cancellationToken);
    return Task.FromResult(Results.Ok(result));
});

app.UseHealthChecks("/healthz");

app.Run();