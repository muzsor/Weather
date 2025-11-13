using System.Reactive;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;
using WeatherCalendar.Services;
using WeatherCalendar.Utils;

namespace WeatherCalendar.ViewModels;

public partial class MainWindowViewModel : ReactiveBase
{
    [Reactive]
    public partial CalendarBaseViewModel CurrentViewModel { get; set; }

    [Reactive]
    public partial bool IsAutoStart { get; set; }

    public ReactiveCommand<Unit, Unit> SwitchAutoStartCommand;

    public ReactiveCommand<Unit, Unit> SwitchTopmostCommand;

    public MainWindowViewModel()
    {
        CurrentViewModel = new MainViewModel();

        var appConfigService = Locator.Current.GetService<AppConfigService>();
        IsAutoStart = appConfigService!.Config.IsAutoStart;

        SwitchAutoStartCommand = ReactiveCommand.Create(() =>
        {
            if (!AppHelper.SetAutoStart(!IsAutoStart))
                return;

            IsAutoStart = !IsAutoStart;
            appConfigService.Config.IsAutoStart = IsAutoStart;
            appConfigService.Save();
        });

        SwitchTopmostCommand = ReactiveCommand.Create(() =>
        {
            appConfigService.Config.IsTopmost = !appConfigService.Config.IsTopmost;
            appConfigService.Save();
            AppHelper.Restart();
        });
    }
}