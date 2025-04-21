using System.ComponentModel.DataAnnotations;

namespace AutoTestWithDocker;

public sealed class EnvSettings
{
    public const string ConfigurationSection = "EnvSettings"; 
    [Required] public required string EnvMessage { get; init; }
    [Required] public required string AppSettingMessage { get; init; }
};