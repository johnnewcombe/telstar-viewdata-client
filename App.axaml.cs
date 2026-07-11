/*
    Copyright (c) 2025 John Newcombe

    This file is part of the Software known as GlassTTY Viewdata Client.

    GlassTTY Viewdata Client is free software: you can redistribute
    it and/or modify it under the terms of the GNU General Public
    License as published by the Free Software Foundation, either
    version 3 of the License, or (at your option) any later version.
    GlassTTY Viewdata Client is distributed in the hope that it will
    be useful, but WITHOUT ANY WARRANTY; without even the implied
    warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
    See the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with the product. If not, see <https://www.gnu.org/licenses/>.

*/

using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using TelstarClient.Views;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using TelstarClient.Comms;
using TelstarClient.ViewModels;

namespace TelstarClient;

public class App : Application
{
    public static IHost Host { get; private set; }
    public static string LogPath { get; private set; }

    public override void Initialize()
    {
// Builds and starts the generic host, which wires up configuration and logging
// for the application. The resulting Host is used elsewhere (e.g. App.Host)
// to resolve ILogger<T> and ILoggerFactory instances via constructor injection.

        Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()

            // Ensures configuration and other content-root-relative paths resolve
            // against the application's install directory rather than the current
            // working directory, which can vary depending on how the app is launched.
            .UseContentRoot(AppContext.BaseDirectory)

            // Loads appsettings.json from the content root. Required (optional: false)
            // since Serilog's configuration below depends on it. reloadOnChange allows
            // log level and sink settings to be adjusted without restarting the app.
            .ConfigureAppConfiguration((_, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })

            // Configures Serilog as the logging provider, replacing the default
            // Microsoft.Extensions.Logging providers.
            .UseSerilog((context, _, loggerConfig) =>
            {
                // Logs are written per-user rather than alongside the application files,
                // since the install location may not be writable and logs should
                // persist independently of any particular install/update.
                LogPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "TelstarClient",
                    "logs",
                    "app-.log");

                // Shared output template for both sinks so log format is consistent
                // between the console (useful while debugging) and the log file
                // (useful for diagnosing issues after the fact). {SourceContext}
                // shows which class/category logged each entry.
                const string template =
                    "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}";

                loggerConfig
                    // Picks up sink/level configuration from appsettings.json (e.g.
                    // minimum log level overrides), merged with the sinks below.
                    .ReadFrom.Configuration(context.Configuration)

                    // Persistent log file, rolled over daily, retained across app runs.
                    .WriteTo.File(LogPath, rollingInterval: RollingInterval.Day, outputTemplate: template)

                    // Live console output, primarily useful when running/debugging
                    // from an IDE or terminal.
                    // Deliberately left in RELEASE builds for convenience.
                    .WriteTo.Console(outputTemplate: template);
            })
            .Build();

        // Loads this Application's XAML (styles, resources, etc.) now that the host
        // and its logging infrastructure are ready to use.
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            //var args = desktop.Args;

            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            var commsClientFactory = new CommsClientFactory(
                App.Host.Services.GetRequiredService<ILoggerFactory>());

            var mainViewModel = new MainWindowViewModel(
                commsClientFactory,
                App.Host.Services.GetRequiredService<ILogger<MainWindowViewModel>>());

            desktop.MainWindow = new MainWindow { DataContext = mainViewModel };

            desktop.ShutdownRequested += (_, _) => { mainViewModel.Dispose(); };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}