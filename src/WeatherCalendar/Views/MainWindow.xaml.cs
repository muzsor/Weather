using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using Splat;
using WeatherCalendar.Services;
using WeatherCalendar.Themes;
using WeatherCalendar.Utils;
using WeatherCalendar.ViewModels;
using Application = System.Windows.Application;

namespace WeatherCalendar.Views;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private CalendarWindow CalendarWindow { get; set; }

    private bool IsSettingsWindowVisible { get; set; }

    public MainWindow()
    {
        InitializeComponent();

        ViewModel = new MainWindowViewModel();

        this.WhenActivated(WhenActivated);
    }

    private void WhenActivated(CompositeDisposable disposable)
    {
        var appConfigService = Locator.Current.GetService<AppConfigService>();
        var theme = Locator.Current.GetService<ITheme>();

        this.SetWindowCanPenetrate(appConfigService!.Config.IsMousePenetrate);
        this.SetWindowToolWindow();

        // 当有多个显示器时置底主界面会消失（目前没有解决方案）
        if (appConfigService.Config.IsTopmost)
        {
            Topmost = true;
        }
        else
        {
            Topmost = false;
            if (Screen.AllScreens.Length == 1)
                this.SetWindowBottom();
        }

        MousePenetrationMenuItem.IsChecked = appConfigService.Config.IsMousePenetrate;
        LockedPositionMenuItem.IsChecked = appConfigService.Config.IsLockedPosition;
        BackgroundTransparentMenuItem.IsChecked = appConfigService.Config.IsBackgroundTransparent;
        TopmostMenuItem.IsChecked = appConfigService.Config.IsTopmost;
        Left = appConfigService.Config.WindowLeft;
        Top = appConfigService.Config.WindowTop;

        AutoStartPackIcon.Visibility = AppHelper.IsAdministrator() ? Visibility.Collapsed : Visibility.Visible;

        MainGrid.Background =
            appConfigService.Config.IsBackgroundTransparent
                ? new SolidColorBrush(Color.FromArgb(1, 0, 0, 0))
                : theme!.MainWindowBackground;

        this.OneWayBind(
                ViewModel,
                model => model.CurrentViewModel,
                window => window.ViewModelViewHost.ViewModel)
            .DisposeWith(disposable);

        this.OneWayBind(
                ViewModel,
                model => model.IsAutoStart,
                view => view.AutoStartMenuItem.IsChecked)
            .DisposeWith(disposable);

        NotifyIcon
            .LeftClickCommand = ReactiveCommand.Create(ShowCalendarWindow);

        CalendarDetailMenuItem
            .Events()
            .Click
            .Do(_ => ShowCalendarWindow())
            .Subscribe()
            .DisposeWith(disposable);

        MousePenetrationMenuItem
            .Events()
            .Checked
            .Do(_ =>
            {
                this.SetWindowCanPenetrate(true);
                appConfigService.Config.IsMousePenetrate = true;
                appConfigService.Save();
            })
            .Subscribe()
            .DisposeWith(disposable);

        MousePenetrationMenuItem
            .Events()
            .Unchecked
            .Do(_ =>
            {
                this.SetWindowCanPenetrate(false);
                appConfigService.Config.IsMousePenetrate = false;
                appConfigService.Save();
            })
            .Subscribe()
            .DisposeWith(disposable);

        LockedPositionMenuItem
            .Events()
            .Checked
            .Do(_ =>
            {
                appConfigService.Config.IsLockedPosition = true;
                appConfigService.Save();
            })
            .Subscribe()
            .DisposeWith(disposable);

        LockedPositionMenuItem
            .Events()
            .Unchecked
            .Do(_ =>
            {
                appConfigService.Config.IsLockedPosition = false;
                appConfigService.Save();
            })
            .Subscribe()
            .DisposeWith(disposable);

        BackgroundTransparentMenuItem
            .Events()
            .Checked
            .Do(_ =>
            {
                MainGrid.Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
                appConfigService.Config.IsBackgroundTransparent = true;
                appConfigService.Save();
            })
            .Subscribe()
            .DisposeWith(disposable);

        BackgroundTransparentMenuItem
            .Events()
            .Unchecked
            .Do(_ =>
            {
                MainGrid.Background = theme!.MainWindowBackground;
                appConfigService.Config.IsBackgroundTransparent = false;
                appConfigService.Save();
            })
            .Subscribe()
            .DisposeWith(disposable);

        this.BindCommand(
                ViewModel!,
                model => model.SwitchTopmostCommand,
                view => view.TopmostMenuItem)
            .DisposeWith(disposable);

        this.BindCommand(
                ViewModel!,
                model => model.SwitchAutoStartCommand,
                view => view.AutoStartMenuItem)
            .DisposeWith(disposable);

        UpdateWeatherMenuItem
            .Events()
            .Click
            .ObserveOn(RxApp.TaskpoolScheduler)
            .Do(_ =>
            {
                var weatherService = Locator.Current.GetService<WeatherService>();
                weatherService!.UpdateWeather();
            })
            .Subscribe()
            .DisposeWith(disposable);

        SettingsMenuItem
            .Events()
            .Click
            .Do(_ => ShowSettingWindow())
            .Subscribe()
            .DisposeWith(disposable);

        CurrentMonthMenuItem
            .Events()
            .Click
            .Select(_ => Unit.Default)
            .InvokeCommand(this,
                window => window.ViewModel.CurrentViewModel.CurrentMonthCommand)
            .DisposeWith(disposable);

        LastMonthMenuItem
            .Events()
            .Click
            .Select(_ => Unit.Default)
            .InvokeCommand(this,
                window => window.ViewModel.CurrentViewModel.LastMonthCommand)
            .DisposeWith(disposable);

        NextMonthMenuItem
            .Events()
            .Click
            .Select(_ => Unit.Default)
            .InvokeCommand(this,
                window => window.ViewModel.CurrentViewModel.NextMonthCommand)
            .DisposeWith(disposable);

        QuitMenuItem
            .Events()
            .Click
            .Do(_ => Application.Current.Shutdown())
            .Subscribe();

        this.Events()
            .MouseLeftButtonDown
            .Do(_ =>
            {
                if (!appConfigService.Config.IsLockedPosition)
                    DragMove();
            })
            .Subscribe()
            .DisposeWith(disposable);

        this.Events()
            .MouseLeftButtonUp
            .Do(_ =>
            {
                var left = (int)Left;
                var top = (int)Top;
                if (left != appConfigService.Config.WindowLeft ||
                    top != appConfigService.Config.WindowTop)
                {
                    appConfigService.Config.WindowLeft = left;
                    appConfigService.Config.WindowTop = top;
                    appConfigService.Save();
                }
            })
            .Subscribe()
            .DisposeWith(disposable);

        this.Events()
            .Loaded
            .Do(_ =>
            {
                this.SetWindowToolWindow();
                this.SetWindowBottom();
            })
            .Subscribe()
            .DisposeWith(disposable);
    }

    private void ShowCalendarWindow()
    {
        if (CalendarWindow == null)
        {
            CalendarWindow = new CalendarWindow();
            CalendarWindow.Closed += (_, _) => CalendarWindow = null;
        }

        if (CalendarWindow.WindowState == WindowState.Minimized)
            CalendarWindow.WindowState = WindowState.Normal;

        if (!CalendarWindow.IsVisible)
            CalendarWindow.Show();

        CalendarWindow.Activate();
    }

    private void ShowSettingWindow()
    {
        if (IsSettingsWindowVisible)
            return;

        IsSettingsWindowVisible = true;
        var window = new SettingsWindow();
        window.Closed += (_, _) => IsSettingsWindowVisible = false;
        window.Show();
    }
}