using System;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;
using Weather;
using WeatherCalendar.Models;
using WeatherCalendar.Services;

// ReSharper disable UnassignedGetOnlyAutoProperty

namespace WeatherCalendar.ViewModels;

public partial class MainViewModel : CalendarBaseViewModel
{
    /// <summary>
    ///     日历
    /// </summary>
    public CalendarViewModel Calendar { get; }

    /// <summary>
    ///     上下班倒计时
    /// </summary>
    public WorkTimerViewModel WorkTimer { get; }

    /// <summary>
    ///     当前时间
    /// </summary>
    [ObservableAsProperty(ReadOnly = false)]
    public partial DateTime CurrentDateTime { get; }

    /// <summary>
    ///     干支年名称（正月）
    /// </summary>
    [ObservableAsProperty(ReadOnly = false)]
    public partial string StemsAndBranchesYearNameOfFirstMonth { get; }

    /// <summary>
    ///     干支年名称（立春）
    /// </summary>
    [ObservableAsProperty(ReadOnly = false)]
    public partial string StemsAndBranchesYearNameOfSpringBegins { get; }

    /// <summary>
    ///     生肖（正月）
    /// </summary>
    [ObservableAsProperty(ReadOnly = false)]
    public partial string ChineseZodiacOfFirstMonth { get; }

    /// <summary>
    ///     生肖（立春）
    /// </summary>
    [ObservableAsProperty(ReadOnly = false)]
    public partial string ChineseZodiacOfSpringBegins { get; }

    /// <summary>
    ///     干支月
    /// </summary>
    [ObservableAsProperty(ReadOnly = false)]
    public partial string StemsAndBranchesMonthName { get; }

    /// <summary>
    ///     干支日
    /// </summary>
    [ObservableAsProperty(ReadOnly = false)]
    public partial string StemsAndBranchesDayName { get; }

    /// <summary>
    ///     农历月信息
    /// </summary>
    [ObservableAsProperty(ReadOnly = false)]
    public partial string LunarMonthInfo { get; }

    /// <summary>
    ///     天气预报
    /// </summary>
    [ObservableAsProperty(ReadOnly = false)]
    public partial WeatherForecast Forecast { get; }

    /// <summary>
    ///     天气图片视图模型
    /// </summary>
    [ObservableAsProperty(ReadOnly = false)]
    public partial ReactiveObject WeatherImageViewModel { get; }

    /// <summary>
    ///     网络信息
    /// </summary>
    [ObservableAsProperty(ReadOnly = false)]
    public partial NetWorkInfo NetWorkInfo { get; }

    /// <summary>
    ///     CPU使用率
    /// </summary>
    [ObservableAsProperty(ReadOnly = false)]
    public partial float CpuLoad { get; }

    /// <summary>
    ///     内存使用率
    /// </summary>
    [ObservableAsProperty(ReadOnly = false)]
    public partial float MemoryLoad { get; }

    /// <summary>
    ///     生肖视图模型
    /// </summary>
    [ObservableAsProperty(ReadOnly = false)]
    public partial ReactiveObject ChineseZodiacViewModel { get; }

    public MainViewModel()
    {
        Calendar = new CalendarViewModel();
        WorkTimer = new WorkTimerViewModel();

        GotoMonthCommand = Calendar.GotoMonthCommand;
        CurrentMonthCommand = Calendar.CurrentMonthCommand;
        LastMonthCommand = Calendar.LastMonthCommand;
        NextMonthCommand = Calendar.NextMonthCommand;
    }

    protected override void OnWhenActivated(CompositeDisposable disposable)
    {
        base.OnWhenActivated(disposable);

        var appService = Locator.Current.GetService<AppService>();

        _currentDateTimeHelper =
            appService
                .TimerPerSecond
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, model => model.CurrentDateTime)
                .DisposeWith(disposable);

        var calendarService = Locator.Current.GetService<CalendarService>();

        _stemsAndBranchesYearNameOfFirstMonthHelper =
            this.WhenAnyValue(x => x.CurrentDateTime)
                .Select(calendarService.GetStemsAndBranchesYearNameOfFirstMonth)
                .ToProperty(this, model => model.StemsAndBranchesYearNameOfFirstMonth)
                .DisposeWith(disposable);

        _stemsAndBranchesYearNameOfSpringBeginsHelper =
            this.WhenAnyValue(x => x.CurrentDateTime)
                .Select(calendarService.GetStemsAndBranchesYearNameOfSpringBegins)
                .ToProperty(this, model => model.StemsAndBranchesYearNameOfSpringBegins)
                .DisposeWith(disposable);

        _chineseZodiacOfFirstMonthHelper =
            this.WhenAnyValue(x => x.CurrentDateTime)
                .Select(calendarService.GetChineseZodiacOfFirstMonth)
                .ToProperty(this, model => model.ChineseZodiacOfFirstMonth)
                .DisposeWith(disposable);

        _chineseZodiacOfSpringBeginsHelper =
            this.WhenAnyValue(x => x.CurrentDateTime)
                .Select(calendarService.GetChineseZodiacOfSpringBegins)
                .ToProperty(this, model => model.ChineseZodiacOfSpringBegins)
                .DisposeWith(disposable);

        _stemsAndBranchesMonthNameHelper =
            this.WhenAnyValue(x => x.CurrentDateTime)
                .Select(calendarService.GetStemsAndBranchesMonthName)
                .ToProperty(this, model => model.StemsAndBranchesMonthName)
                .DisposeWith(disposable);

        _stemsAndBranchesDayNameHelper =
            this.WhenAnyValue(x => x.CurrentDateTime)
                .Select(calendarService.GetStemsAndBranchesDayName)
                .ToProperty(this, model => model.StemsAndBranchesDayName)
                .DisposeWith(disposable);

        _lunarMonthInfoHelper =
            this.WhenAnyValue(x => x.CurrentDateTime)
                .Select(calendarService.GetLunarMonthInfo)
                .ToProperty(this, model => model.LunarMonthInfo)
                .DisposeWith(disposable);

        var chineseZodiacService = Locator.Current.GetService<IChineseZodiacService>();

        _chineseZodiacViewModelHelper =
            this.WhenAnyValue(x => x.ChineseZodiacOfFirstMonth)
                .Select(chineseZodiacService.GetChineseZodiacViewModel)
                .ToProperty(this, model => model.ChineseZodiacViewModel)
                .DisposeWith(disposable);

        var weatherService = Locator.Current.GetService<WeatherService>();

        _forecastHelper =
            weatherService
                .WhenAnyValue(x => x.Forecast)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, model => model.Forecast)
                .DisposeWith(disposable);

        _weatherImageViewModelHelper =
            this.WhenAnyValue(x => x.Forecast)
                .Select(w => w?.GetCurrentWeather())
                .Select(w =>
                {
                    if (w == null)
                        return null;

                    var (weather, isNight) = w.Value;
                    if (weather == null)
                        return null;

                    return Locator.Current.GetService<IWeatherImageService>()
                        ?.GetWeatherImageViewModel(weather.Weather, isNight);
                })
                .ToProperty(this, model => model.WeatherImageViewModel)
                .DisposeWith(disposable);

        var systemInfoService = Locator.Current.GetService<SystemInfoService>();

        _netWorkInfoHelper =
            systemInfoService
                .WhenAnyValue(x => x.NetWorkInfo)
                .ToProperty(this, model => model.NetWorkInfo, false, RxApp.MainThreadScheduler)
                .DisposeWith(disposable);

        _cpuLoadHelper =
            systemInfoService
                .WhenAnyValue(x => x.CpuLoad)
                .ToProperty(this, model => model.CpuLoad, false, RxApp.MainThreadScheduler)
                .DisposeWith(disposable);

        _memoryLoadHelper =
            systemInfoService
                .WhenAnyValue(x => x.MemoryLoad)
                .ToProperty(this, model => model.MemoryLoad, false, RxApp.MainThreadScheduler)
                .DisposeWith(disposable);
    }
}