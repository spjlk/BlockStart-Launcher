using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using CmlLib.Core;
using CmlLib.Core.Auth.Microsoft;
using CmlLib.Core.ProcessBuilder;

namespace BlockStartLauncher;

public partial class MainWindow : Window
{
    private readonly MinecraftLauncher _launcher;
    private CmlLib.Core.Auth.MSession? _session;
    private bool _loadingVersions;

    public MainWindow()
    {
        InitializeComponent();
        var gamePath = new MinecraftPath(
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                         ".blockstart", "minecraft"));
        _launcher = new MinecraftLauncher(gamePath);
        Loaded += async (_, _) => await LoadVersionsAsync();
    }

    private async Task LoadVersionsAsync()
    {
        if (_loadingVersions) return;
        _loadingVersions = true;
        try
        {
            SetBusy(true, "Loading Minecraft version manifest...");
            var versions = await _launcher.GetAllVersionsAsync();
            var releases = versions
                .Where(v => string.Equals(v.Type.ToString(), "Release", StringComparison.OrdinalIgnoreCase))
                .Select(v => v.Name)
                .Distinct()
                .ToList();

            await Dispatcher.InvokeAsync(() =>
            {
                VersionBox.ItemsSource = releases;
                if (releases.Count > 0) VersionBox.SelectedIndex = 0;
            });
            SetStatus($"Loaded {releases.Count} release versions.");
        }
        catch (Exception ex)
        {
            SetStatus("Could not load versions: " + ex.Message);
        }
        finally
        {
            _loadingVersions = false;
            SetBusy(false);
        }
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            LoginButton.IsEnabled = false;
            SetStatus("Opening Microsoft sign-in...");
            var loginHandler = JELoginHandlerBuilder.BuildDefault();
            _session = await loginHandler.Authenticate();
            AccountLabel.Text = "Signed in: " + (_session.Username ?? "Microsoft account");
            SetStatus("Microsoft authentication successful.");
        }
        catch (Exception ex)
        {
            _session = null;
            AccountLabel.Text = "Not signed in";
            SetStatus("Sign-in failed: " + ex.Message);
            MessageBox.Show(this, ex.Message, "Microsoft sign-in failed",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            LoginButton.IsEnabled = true;
        }
    }

    private async void PlayButton_Click(object sender, RoutedEventArgs e)
    {
        var edition = (EditionBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
        if (edition == "Bedrock Edition")
        {
            LaunchBedrock();
            return;
        }

        if (_session is null)
        {
            MessageBox.Show(this, "Please sign in with the Microsoft account that owns Minecraft: Java Edition.",
                "Sign-in required", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (VersionBox.SelectedItem is not string version)
        {
            SetStatus("Choose a Minecraft version first.");
            return;
        }

        try
        {
            SetBusy(true, $"Preparing Minecraft {version}. First launch may download game files...");
            var options = new MLaunchOption
            {
                Session = _session,
                MaximumRamMb = (int)RamSlider.Value * 1024
            };

            var process = await _launcher.InstallAndBuildProcessAsync(version, options);
            process.Start();
            SetStatus($"Minecraft {version} started.");
        }
        catch (Exception ex)
        {
            SetStatus("Launch failed: " + ex.Message);
            MessageBox.Show(this, ex.ToString(), "Minecraft launch failed",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private static void LaunchBedrock()
    {
        try
        {
            // The minecraft: protocol is registered by supported Bedrock installations.
            Process.Start(new ProcessStartInfo("minecraft:") { UseShellExecute = true });
        }
        catch
        {
            try
            {
                Process.Start(new ProcessStartInfo(
                    "ms-windows-store://pdp/?ProductId=9NBLGGH2JHXJ") { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open Minecraft for Windows. Install or repair it using the Xbox app or Microsoft Store.\n\n" + ex.Message,
                    "Bedrock Edition", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

    private void EditionBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded || VersionBox is null) return;
        bool java = (EditionBox.SelectedItem as ComboBoxItem)?.Content?.ToString() == "Java Edition";
        VersionBox.IsEnabled = java;
        RamSlider.IsEnabled = java;
        PlayButton.Content = java ? "▶   PLAY JAVA" : "▶   PLAY BEDROCK";
        SetStatus(java ? "Java Edition selected." : "Bedrock Edition selected.");
    }

    private void RamSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (RamValue is not null) RamValue.Text = $"{(int)e.NewValue} GB";
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e) =>
        await LoadVersionsAsync();

    private void SetStatus(string message) =>
        Dispatcher.Invoke(() => StatusText.Text = message);

    private void SetBusy(bool busy, string? message = null)
    {
        Dispatcher.Invoke(() =>
        {
            PlayButton.IsEnabled = !busy;
            if (message is not null) StatusText.Text = message;
        });
    }
}
