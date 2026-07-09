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

        Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .UseContentRoot(AppContext.BaseDirectory)
            .ConfigureAppConfiguration((_, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            
            .UseSerilog((context, _, loggerConfig) =>
            {
                LogPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "TelstarClient",
                    "logs",
                    "app-.log");
                const string template = "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}";
                loggerConfig
                    .ReadFrom.Configuration(context.Configuration)
                    .WriteTo.File(LogPath, rollingInterval: RollingInterval.Day, outputTemplate: template)
                    .WriteTo.Console(outputTemplate: template);
            })

            .Build();
        
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
//            var tcpClient = new TcpClient(
 //               App.Host.Services.GetRequiredService<ILogger<TcpClient>>());
            var commsClientFactory = new CommsClientFactory(
                App.Host.Services.GetRequiredService<ILoggerFactory>());
            
            var mainViewModel = new MainWindowViewModel(
                commsClientFactory,
                App.Host.Services.GetRequiredService<ILogger<MainWindowViewModel>>());

            desktop.MainWindow = new MainWindow { DataContext =  mainViewModel };
            
            desktop.ShutdownRequested += (_, _) =>
            {
                mainViewModel.Dispose();
            };
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