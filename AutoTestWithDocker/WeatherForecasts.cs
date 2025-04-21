namespace AutoTestWithDocker;

public sealed record WeatherForecasts(DateOnly Date, int TemperatureC, string? Summary);

public sealed record ApiResponse(string EnvMessage, string AppSettingMessage);