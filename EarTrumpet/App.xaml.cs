﻿using EarTrumpet.DataModel;
using EarTrumpet.Misc;
using EarTrumpet.Services;
using EarTrumpet.ViewModels;
using EarTrumpet.Views;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace EarTrumpet
{
    public partial class App
    {
        private MainViewModel _viewModel;
        private TrayIcon _trayIcon;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Trace.WriteLine("Application_Startup");
            if (!SingleInstanceAppMutex.TakeExclusivity())
            {
                Trace.WriteLine("TakeExclusivity failed");
                Current.Shutdown();
                return;
            }

            Exit += (_, __) => SingleInstanceAppMutex.ReleaseExclusivity();

            StartupUWPDialogDisplayService.ShowIfAppropriate();

            ((ThemeManager)Resources["ThemeManager"]).SetTheme(AppSpecificThemes.GetThemeBuildData());

            var deviceManager = DataModelFactory.CreateAudioDeviceManager();
            DiagnosticsService.AdviseManager(deviceManager);

            _viewModel = new MainViewModel(deviceManager);
            HotkeyService.Register(SettingsService.Hotkey);
            HotkeyService.KeyPressed += (_, __) => _viewModel.OpenFlyout();

            var flyoutViewModel = new FlyoutViewModel(_viewModel, deviceManager);
            var flyoutWindow = new FlyoutWindow(_viewModel, flyoutViewModel);

            var trayViewModel = new TrayViewModel(_viewModel, deviceManager);
            _trayIcon = new TrayIcon(deviceManager, trayViewModel);

#if VSDEBUG
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                new DebugWindow().Show();
            }
#endif
            Trace.WriteLine("Application_Startup Exit");
        }
    }
}
