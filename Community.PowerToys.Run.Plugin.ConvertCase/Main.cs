using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using ManagedCommon;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.ConvertCase;

/// <summary>
///     Main class of this plugin that implement all used interfaces.
/// </summary>
public class Main : IPlugin, IContextMenu, IDisposable
{
    /// <summary>
    ///     ID of the plugin.
    /// </summary>
    public static string PluginID => "297EF27906A94E45AFCE92DEC6119E20";

    private PluginInitContext Context { get; set; }

    private string IconPath { get; set; }

    private bool Disposed { get; set; }

    /// <summary>
    ///     Return a list context menu entries for a given <see cref="Result" /> (shown at the right side of the result).
    /// </summary>
    /// <param name="selectedResult">The <see cref="Result" /> for the list with context menu entries.</param>
    /// <returns>A list context menu entries.</returns>
    public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
    {
        if (selectedResult.ContextData is string search)
            return
            [
                new ContextMenuResult
                {
                    PluginName = Name,
                    Title = "Copy to clipboard (Ctrl+C)",
                    FontFamily = "Segoe MDL2 Assets",
                    Glyph = "\xE8C8", // Copy
                    AcceleratorKey = Key.C,
                    AcceleratorModifiers = ModifierKeys.Control,
                    Action = _ =>
                    {
                        Clipboard.SetDataObject(search);
                        return true;
                    }
                }
            ];

        return [];
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Name of the plugin.
    /// </summary>
    public string Name => "ConvertCase";

    /// <summary>
    ///     Description of the plugin.
    /// </summary>
    public string Description => "ConvertCase Description";

    /// <summary>
    ///     Return a filtered list, based on the given query.
    /// </summary>
    /// <param name="query">The query to filter the list.</param>
    /// <returns>A filtered list, can be empty when nothing was found.</returns>
    public List<Result> Query(Query query)
    {
        var search = query.Search;

        if (search.Length == 0)
        {
            return [];
        }

        return
        [
            GenerateResult(search, Underscore(search), "snake_case"),
            GenerateResult(search, ToUpperSnakeCase(search), "Upper_snake_case"),
            GenerateResult(search, Pascalize(search), "PascalCase")
        ];
    }

    private static string ToUpperSnakeCase(string search)
    {
        var upperSnakeCase = Underscore(search);
        upperSnakeCase = char.ToUpper(upperSnakeCase[0]) + upperSnakeCase.Substring(1);
        return upperSnakeCase;
    }

    /// <summary>
    ///     Initialize the plugin with the given <see cref="PluginInitContext" />.
    /// </summary>
    /// <param name="context">The <see cref="PluginInitContext" /> for this plugin.</param>
    public void Init(PluginInitContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        Context.API.ThemeChanged += OnThemeChanged;
        UpdateIconPath(Context.API.GetCurrentTheme());
    }

    private static string Pascalize(string input)
    {
        return Regex.Replace(input, @"(?:[ _-]+|^)([a-zA-Z])", match => match
            .Groups[1]
            .Value.ToUpper());
    }

    private static string Underscore(string input)
    {
        return Regex
            .Replace(
                Regex.Replace(
                    Regex.Replace(input, @"([\p{Lu}]+)([\p{Lu}][\p{Ll}])", "$1_$2"), @"([\p{Ll}\d])([\p{Lu}])",
                    "$1_$2"), @"[-\s]", "_")
            .ToLower();
    }

    private Result GenerateResult(string search, string value, string subTitle)
    {
        return new Result
        {
            QueryTextDisplay = search,
            IcoPath = IconPath,
            Title = value,
            SubTitle = subTitle,
            Action = _ =>
            {
                Clipboard.SetDataObject(value);
                return true;
            },
            ContextData = search
        };
    }

    /// <summary>
    ///     Wrapper method for <see cref="Dispose()" /> that dispose additional objects and events form the plugin itself.
    /// </summary>
    /// <param name="disposing">Indicate that the plugin is disposed.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (Disposed || !disposing) return;

        if (Context?.API != null) Context.API.ThemeChanged -= OnThemeChanged;

        Disposed = true;
    }

    private void UpdateIconPath(Theme theme)
    {
        IconPath = theme == Theme.Light || theme == Theme.HighContrastWhite
            ? "Images/convertcase.light.png"
            : "Images/convertcase.dark.png";
    }

    private void OnThemeChanged(Theme currentTheme, Theme newTheme)
    {
        UpdateIconPath(newTheme);
    }
}