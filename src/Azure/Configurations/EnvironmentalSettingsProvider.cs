// Copyright ChrisTanev. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Locksmith.NET.Azure.Configurations;

public class EnvironmentalSettingsProvider : IEnvironmentalSettingsProvider
{
    public string GetEnvironmentalSetting(string? key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));

        return Environment.GetEnvironmentVariable(key) ?? throw new InvalidOperationException($"The environmental setting '{key}' is not set.");
    }

    public void SetEnvironmentalSetting(string? key, string? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));

        Environment.SetEnvironmentVariable(key, value);
    }
}
