using System.Globalization;
using System.Reflection;
using System.Resources;

namespace FluentResultsSerialization;
/// <summary>
/// Provides localization support by retrieving messages based on the current culture.
/// </summary>
internal static class LocalizationHelper
{
    private static readonly ResourceManager _resourceManager =
        new($"{typeof(LocalizationHelper).Namespace}.Resources.Messages", Assembly.GetExecutingAssembly());

    /// <summary>
    /// Gets a message identified by the <paramref name="key"/>.
    /// </summary>
    /// <param name="key">ID of the message.</param>
    /// <returns>A message.</returns>
    public static string GetMessage(string key) =>
        _resourceManager.GetString(key, CultureInfo.CurrentCulture) ?? key;
}
