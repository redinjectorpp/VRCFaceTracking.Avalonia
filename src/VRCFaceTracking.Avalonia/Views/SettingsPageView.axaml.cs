using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using VRCFaceTracking.Contracts.Services;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Services;

namespace VRCFaceTracking.Avalonia.Views;

public partial class SettingsPageView : UserControl
{
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly ILanguageSelectorService _languageSelectorService;
    private readonly IMainService _mainService;
    private readonly ComboBox _themeComboBox;
    private readonly ComboBox _langComboBox;

    public SettingsPageView()
    {
        InitializeComponent();

        _themeSelectorService = Ioc.Default.GetService<IThemeSelectorService>()!;
        _themeComboBox = this.Find<ComboBox>("ThemeCombo")!;
        _themeComboBox.SelectionChanged += ThemeComboBox_SelectionChanged;

        _languageSelectorService = Ioc.Default.GetService<ILanguageSelectorService>()!;
        _langComboBox = this.Find<ComboBox>("LangCombo")!;
        _langComboBox.SelectionChanged += LangComboBox_SelectionChanged;

        UpdateThemes();

        _mainService = Ioc.Default.GetService<IMainService>()!;

        if (_themeSelectorService.Theme is null)
        {
            _themeSelectorService.SetThemeAsync(ThemeVariant.Default);
            return;
        }

        if (string.IsNullOrEmpty(_languageSelectorService.Language))
        {
            _languageSelectorService.SetLanguageAsync(LanguageSelectorService.DefaultLanguage);
            return;
        }

        int index = _themeSelectorService.Theme.ToString() switch
        {
            "DefaultTheme" => 0,
            "Light" => 1,
            "Dark" => 2,
            _ => 0
        };
        _themeComboBox.SelectedIndex = index;

        index = _languageSelectorService.Language switch
        {
            "DefaultLanguage" => 0,
            "en" => 1,
            "es" => 2,
            "ja" => 3,
            "pl" => 4,
            "zh" => 5,
            _ => 0
        };
        _langComboBox.SelectedIndex = index;
    }

    ~SettingsPageView()
    {
        _themeComboBox.SelectionChanged -= ThemeComboBox_SelectionChanged;
    }

    private void ThemeComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_themeComboBox.SelectedItem is not ComboBoxItem comboBoxItem)
            return;

        ThemeVariant variant = ThemeVariant.Default;
        variant = comboBoxItem!.Name switch
        {
            "DefaultTheme" => ThemeVariant.Default,
            "Light" => ThemeVariant.Light,
            "Dark" => ThemeVariant.Dark,
            _ => variant
        };
        Dispatcher.UIThread.InvokeAsync(async () => await _themeSelectorService.SetThemeAsync(variant));
    }

    private void LangComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var item = _langComboBox.SelectedItem as ComboBoxItem;
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await _languageSelectorService.SetLanguageAsync(item!.Name);
            UpdateThemes();
        });
    }

    // Workaround for https://github.com/AvaloniaUI/Avalonia/issues/4460
    private void UpdateThemes()
    {
        var selectedIndex = _themeComboBox.SelectedIndex;
        _themeComboBox.Items.Clear();
        _themeComboBox.Items.Add(new ComboBoxItem { Content=Assets.Resources.Settings_Theme_Default_Content, Name="DefaultTheme" });
        _themeComboBox.Items.Add(new ComboBoxItem { Content=Assets.Resources.Settings_Theme_Light_Content, Name="Light" });
        _themeComboBox.Items.Add(new ComboBoxItem { Content=Assets.Resources.Settings_Theme_Dark_Content, Name="Dark" });
        _themeComboBox.SelectedIndex = selectedIndex;
    }

    private void Calibration_OnClick(object? sender, RoutedEventArgs e)
    {

    }

    private void ReInit_OnClick(object? sender, RoutedEventArgs e)
    {
        _mainService.Teardown();
        _mainService.InitializeAsync();
    }

    private void Reset_OnClick(object? sender, RoutedEventArgs e)
    {
        // Create a file in the VRCFT folder called "reset"
        // This will cause the app to reset on the next launch
        File.Create(Path.Combine(VRCFaceTracking.Core.Utils.PersistentDataDirectory, "reset"));
    }
}

