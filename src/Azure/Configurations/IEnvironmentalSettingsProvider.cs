namespace Locksmith.NET.Azure.Configurations;

public interface IEnvironmentalSettingsProvider
{
    string GetEnvironmentalSetting(string? key);

    void SetEnvoronmentalSetting(string? key, string? value);
}