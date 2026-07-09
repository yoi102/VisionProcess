using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace VisionProcess.wpf.Helpers;


public static class SnackbarIdentifierAssist
{
    private static readonly Dictionary<object, List<Snackbar>> _snackbarGroups = [];

    public static IReadOnlyDictionary<object, IReadOnlyList<Snackbar>> SnackbarGroups =>
        _snackbarGroups.ToDictionary(
            pair => pair.Key,
            pair => (IReadOnlyList<Snackbar>)pair.Value.ToArray());

    internal static IReadOnlyCollection<Snackbar> GetSnackbars(object identifier)
    {
        if (!_snackbarGroups.TryGetValue(identifier, out var snackbars))
            return [];

        return snackbars.Where(snackbar => snackbar.IsLoaded).Distinct().ToArray();
    }

    internal static IReadOnlyCollection<Snackbar> GetAllSnackbars()
    {
        return _snackbarGroups.Values
            .SelectMany(snackbars => snackbars)
            .Where(snackbar => snackbar.IsLoaded)
            .Distinct()
            .ToArray();
    }

    public static readonly DependencyProperty SnackbarIdentifierProperty =
        DependencyProperty.RegisterAttached(
            "SnackbarIdentifier",
            typeof(object),
            typeof(SnackbarIdentifierAssist),
            new PropertyMetadata(null, OnSnackbarIdentifierChanged));

    public static void SetSnackbarIdentifier(DependencyObject element, object? value)
    {
        element.SetValue(SnackbarIdentifierProperty, value);
    }

    public static object? GetSnackbarIdentifier(DependencyObject element)
    {
        return element.GetValue(SnackbarIdentifierProperty);
    }

    private static void OnSnackbarIdentifierChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Snackbar snackbar || Equals(e.OldValue, e.NewValue))
            return;

        if (e.OldValue is not null)
        {
            snackbar.Loaded -= Snackbar_Loaded;
            snackbar.Unloaded -= Snackbar_Unloaded;
            RemoveSnackbar(e.OldValue, snackbar);
        }

        if (e.NewValue is not null)
        {
            snackbar.Loaded += Snackbar_Loaded;
            snackbar.Unloaded += Snackbar_Unloaded;

            if (snackbar.IsLoaded)
                AddSnackbar(e.NewValue, snackbar);
        }
    }

    private static void Snackbar_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is not Snackbar snackbar)
            return;

        var identifier = GetSnackbarIdentifier(snackbar);
        if (identifier is not null)
            AddSnackbar(identifier, snackbar);
    }

    private static void Snackbar_Unloaded(object sender, RoutedEventArgs e)
    {
        if (sender is not Snackbar snackbar)
            return;

        var identifier = GetSnackbarIdentifier(snackbar);
        if (identifier is not null)
            RemoveSnackbar(identifier, snackbar);
    }

    private static void AddSnackbar(object identifier, Snackbar snackbar)
    {
        if (!_snackbarGroups.TryGetValue(identifier, out var list))
        {
            list = [];
            _snackbarGroups[identifier] = list;
        }

        if (!list.Contains(snackbar))
            list.Add(snackbar);
    }

    private static void RemoveSnackbar(object identifier, Snackbar snackbar)
    {
        if (!_snackbarGroups.TryGetValue(identifier, out var list))
            return;

        list.Remove(snackbar);
        if (list.Count == 0)
            _snackbarGroups.Remove(identifier);
    }
}
