using System.Windows;
using ReactiveUI.Builder;
using Splat;
using Splat.NLog;
using WeatherCalendar.Services;

namespace WeatherCalendar;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        RxAppBuilder.CreateReactiveUIBuilder()
            .WithWpf()
            .WithViewsFromAssembly(typeof(App).Assembly)
            .Build();

        Locator.CurrentMutable.UseNLogWithWrappingFullLogger();

        var appService = new AppService();
        Locator.CurrentMutable.RegisterConstant(appService);

        appService.Initial();
    }
}