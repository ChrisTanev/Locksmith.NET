namespace Locksmith.NET.Azure.Configurations;

public interface IEnvironmentalSettingsProvider
{
    string GetEnvironmentalSetting(string? key);

    void SetEnvironmentalSetting(string? key, string? value);
}