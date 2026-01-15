using System.Text;
using System.Windows;
using NLog;
using NLog.Targets;
using ReactiveUI.Builder;
using Splat;
using Splat.NLog;
using WeatherCalendar.Services;
using LogLevel = NLog.LogLevel;

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
        NLogConfigure();

        var appService = new AppService();
        Locator.CurrentMutable.RegisterConstant(appService);

        appService.Initial();
    }

    private static void NLogConfigure()
    {
        // 全局日志级别
#if DEBUG
        LogManager.GlobalThreshold = LogLevel.Trace;
#else
        LogManager.GlobalThreshold = LogLevel.Info;
#endif
        LogManager.Setup().LoadConfiguration(builder =>
        {
            // ReactiveUI 和 Avalonia 的日志级别设置
            builder.ForLogger("ReactiveUI.*").WriteToNil(LogLevel.Warn);
            builder.ForLogger("Avalonia.*").WriteToNil(LogLevel.Warn);
            // 忽略 Splat 的初始化日志
            builder.ForLogger("Splat.*")
                .FilterDynamicIgnore(info => info.Message.StartsWith("Initializing to"), true)
                .WriteToNil();
            // 全都写入控制台
            builder.ForLogger().WriteToConsole(encoding: Encoding.UTF8);
            // Trace 和 Debug 级别的日志写入文件
            builder.ForLogger().FilterDynamicIgnore(info =>
                    info.LoggerName.StartsWith("ReactiveUI"))
                .WriteTo(
                    new FileTarget
                    {
                        Encoding = Encoding.UTF8,
                        FileName = "${basedir}/logs/programs/${shortdate}.log",
                        Layout =
                            "${longdate} ${uppercase:${level}} ${logger} ${message} ${exception:format=Message} ${exception:format=StackTrace}"
                    });
        });
    }
}