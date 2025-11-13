using System.Linq;
using System.Reactive;
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

public partial class SelectCityWindowViewModel : ReactiveBase
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
    ///     更新成功
    /// </summary>
    public Interaction<Unit, Unit> UpdateSuccessInteraction { get; }

    /// <summary>
    ///     更新天气命令
    /// </summary>
    public ReactiveCommand<Unit, Unit> UpdateWeatherCommand;

    public SelectCityWindowViewModel()
    {
        var weatherService = Locator.Current.GetService<WeatherService>();

        AllCities = weatherService.GetCities();

        var canUpdateWeatherCommandExecute =
            this.WhenAnyValue(x => x.SelectedCityInfo)
                .Select(c => c != null);

        UpdateWeatherCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (SelectedCityInfo == null)
                return;

#pragma warning disable 4014
            Task.Run(() => weatherService.UpdateWeather(SelectedCityInfo));
#pragma warning restore 4014

            var appConfigService = Locator.Current.GetService<AppConfigService>();
            appConfigService.Config.CityKey = SelectedCityInfo?.CityKey;
            appConfigService.Save();

            await UpdateSuccessInteraction!.Handle(Unit.Default);
        }, canUpdateWeatherCommandExecute);

        UpdateSuccessInteraction = new Interaction<Unit, Unit>();
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
    }
}