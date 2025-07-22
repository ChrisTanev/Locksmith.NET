// Copyright ChrisTanev. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Locksmith.NET.Azure.Configurations;

public interface IEnvironmentalSettingsProvider
{
    string GetEnvironmentalSetting(string? key);

    void SetEnvironmentalSetting(string? key, string? value);
}
