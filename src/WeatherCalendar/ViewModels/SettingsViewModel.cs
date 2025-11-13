using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;
using Weather;
using WeatherCalendar.Services;

// ReSharper disable UnassignedGetOnlyAutoProperty

namespace WeatherCalendar.ViewModels;

public partial class SettingsViewModel : ReactiveBase
{
    private CityKeyInfo[] AllCities { get; }

    /// <summary>
    ///     省份
    /// </summary>
    [ObservableAsProperty(ReadOnly = false)]
    public partial string[] Provinces { get; }

    /// <summary>
    ///     地区
    /// </summary>
    [ObservableAsProperty(ReadOnly = false)]
    public partial string[] Districts { get; }

    /// <summary>
    ///     城市
    /// </summary>
    [ObservableAsProperty(ReadOnly = false)]
    public partial string[] Cities { get; }

    /// <summary>
    ///     选中的省份
    /// </summary>
    [Reactive]
    public partial string SelectedProvince { get; set; }

    /// <summary>
    ///     选中的地区
    /// </summary>
    [Reactive]
    public partial string SelectedDistrict { get; set; }

    /// <summary>
    ///     选中的城市
    /// </summary>
    [Reactive]
    public partial string SelectedCity { get; set; }

    /// <summary>
    ///     选中的城市信息
    /// </summary>
    [ObservableAsProperty(ReadOnly = false)]
    public partial CityKeyInfo SelectedCityInfo { get; }

    /// <summary>
    ///     是否显示上下班倒计时
    /// </summary>
    [Reactive]
    public partial bool IsWorkTimerVisible { get; set; }

    /// <summary>
    ///     上下班倒计时开始时间
    /// </summary>
    [Reactive]
    public partial TimeSpan WorkTimerStartTime { get; set; }

    /// <summary>
    ///     上下班倒计时结束时间
    /// </summary>
    [Reactive]
    public partial TimeSpan WorkTimerEndTime { get; set; }

    /// <summary>
    ///     是否为自定义天气图标
    /// </summary>
    [Reactive]
    public partial bool IsCustomWeatherIcon { get; set; }

    /// <summary>
    ///     自定义天气图标路径
    /// </summary>
    [Reactive]
    public partial string CustomWeatherIconPath { get; set; }

    public SettingsViewModel()
    {
        var weatherService = Locator.Current.GetService<WeatherService>();

        AllCities = weatherService.GetCities();
    }

    protected override void OnWhenActivated(CompositeDisposable disposable)
    {
        base.OnWhenActivated(disposable);

        var weatherService = Locator.Current.GetService<WeatherService>();

        _provincesHelper =
            Observable
                .Return(
                    AllCities
                        .Select(c => c.Province)
                        .Distinct()
                        .ToArray())
                .ToProperty(this, model => model.Provinces)
                .DisposeWith(disposable);

        _districtsHelper =
            this.WhenAnyValue(x => x.SelectedProvince)
                .Select(p =>
                    AllCities
                        .Where(c => c.Province == p)
                        .Select(c => c.District)
                        .Distinct()
                        .ToArray())
                .Do(_ => SelectedDistrict = "")
                .ToProperty(this, model => model.Districts)
                .DisposeWith(disposable);

        _citiesHelper =
            this.WhenAnyValue(
                    x => x.SelectedProvince,
                    x => x.SelectedDistrict)
                .Select(p =>
                    AllCities
                        .Where(c => c.Province == p.Item1 && c.District == p.Item2)
                        .Select(c => c.City)
                        .Distinct()
                        .ToArray())
                .Do(_ => SelectedCity = "")
                .ToProperty(this, model => model.Cities)
                .DisposeWith(disposable);

        SelectedProvince = weatherService.City?.Province;
        SelectedDistrict = weatherService.City?.District;
        SelectedCity = weatherService.City?.City;

        _selectedCityInfoHelper =
            this.WhenAnyValue(
                    x => x.SelectedProvince,
                    x => x.SelectedDistrict,
                    x => x.SelectedCity,
                    (province, district, city) =>
                        AllCities.FirstOrDefault(c =>
                            c.Province == province &&
                            c.District == district &&
                            c.City == city))
                .ToProperty(this, model => model.SelectedCityInfo)
                .DisposeWith(disposable);

        this.WhenAnyValue(x => x.SelectedCityInfo)
            .Where(c => c != null)
            .Do(city =>
            {
                Task.Run(() => weatherService.UpdateWeather(city));

                Task.Run(() =>
                {
                    var configService = Locator.Current.GetService<AppConfigService>();
                    configService.Config.CityKey = city?.CityKey;
                    configService.Save();
                });
            })
            .Subscribe()
            .DisposeWith(disposable);

        var appService = Locator.Current.GetService<AppService>();
        var appConfigService = Locator.Current.GetService<AppConfigService>();
        var workTimerService = Locator.Current.GetService<WorkTimerService>();

        IsWorkTimerVisible = appConfigService.Config.IsShowWorkTimer;
        WorkTimerStartTime = appConfigService.Config.WorkStartTime;
        WorkTimerEndTime = appConfigService.Config.WorkEndTime;
        IsCustomWeatherIcon = appConfigService.Config.IsCustomWeatherIcon;
        CustomWeatherIconPath = appConfigService.Config.CustomWeatherIconPath;

        this.WhenAnyValue(x => x.IsWorkTimerVisible)
            .Do(isVisible =>
            {
                workTimerService.IsVisible = isVisible;

                appConfigService.Config.IsShowWorkTimer = isVisible;
                appConfigService.Save();
            })
            .Subscribe()
            .DisposeWith(disposable);

        this.WhenAnyValue(x => x.WorkTimerStartTime)
            .Throttle(TimeSpan.FromMilliseconds(100))
            .Do(time =>
            {
                workTimerService.StartTime = time;

                appConfigService.Config.WorkStartTime = time;
                appConfigService.Save();
            })
            .Subscribe()
            .DisposeWith(disposable);

        this.WhenAnyValue(x => x.WorkTimerEndTime)
            .Throttle(TimeSpan.FromMilliseconds(100))
            .Do(time =>
            {
                workTimerService.EndTime = time;

                appConfigService.Config.WorkEndTime = time;
                appConfigService.Save();
            })
            .Subscribe()
            .DisposeWith(disposable);

        this.WhenAnyValue(x => x.IsCustomWeatherIcon)
            .Do(isCustom =>
            {
                appService.LoadWeatherIcon(isCustom, CustomWeatherIconPath);

                appConfigService.Config.IsCustomWeatherIcon = isCustom;
                appConfigService.Save();
            })
            .Subscribe()
            .DisposeWith(disposable);

        this.WhenAnyValue(x => x.CustomWeatherIconPath)
            .Do(path =>
            {
                appService.LoadWeatherIcon(IsCustomWeatherIcon, path);

                appConfigService.Config.CustomWeatherIconPath = path;
                appConfigService.Save();
            })
            .Subscribe()
            .DisposeWith(disposable);
    }
}