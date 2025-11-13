using System;
using System.Collections.Generic;
using System.Reactive;
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

public partial class DayViewModel : ReactiveBase
{
    /// <summary>
    ///     所有视图模型
    /// </summary>
    private static readonly List<WeakReference<DayViewModel>> AllDayViewModels = new();

    /// <summary>
    ///     是否有效
    /// </summary>
    [Reactive]
    public partial bool IsValid { get; set; }

    /// <summary>
    ///     日期信息
    /// </summary>
    [Reactive]
    public partial DateInfo Date { get; set; }

    /// <summary>
    ///     是否为当日
    /// </summary>
    [Reactive]
    public partial bool IsCurrentDay { get; private set; }

    /// <summary>
    ///     是否为当前月
    /// </summary>
    [Reactive]
    public partial bool IsCurrentPageMonth { get; set; }

    /// <summary>
    ///     天气信息
    /// </summary>
    [Reactive]
    public partial ForecastInfo Forecast { get; set; }

    /// <summary>
    ///     生肖视图模型
    /// </summary>
    [ObservableAsProperty(ReadOnly = false)]
    public partial ReactiveObject ChineseZodiacViewModel { get; }

    /// <summary>
    ///     白天天气图片视图模型
    /// </summary>
    [ObservableAsProperty(ReadOnly = false)]
    public partial ReactiveObject DayWeatherImageViewModel { get; }

    /// <summary>
    ///     夜间天气图片视图模型
    /// </summary>
    [ObservableAsProperty(ReadOnly = false)]
    public partial ReactiveObject NightWeatherImageViewModel { get; }

    /// <summary>
    ///     公历日期
    /// </summary>
    [ObservableAsProperty(ReadOnly = false)]
    public partial string DayName { get; }

    /// <summary>
    ///     农历日期
    /// </summary>
    [ObservableAsProperty(ReadOnly = false)]
    public partial string LunarDayName { get; }

    /// <summary>
    ///     节气
    /// </summary>
    [ObservableAsProperty(ReadOnly = false)]
    public partial string SolarTermName { get; }

    /// <summary>
    ///     节假日
    /// </summary>
    [ObservableAsProperty(ReadOnly = false)]
    public partial string FestivalName { get; }

    /// <summary>
    ///     是否为中国节假日
    /// </summary>
    [ObservableAsProperty(ReadOnly = false)]
    public partial bool IsChineseFestival { get; }

    /// <summary>
    ///     是否为周末
    /// </summary>
    [ObservableAsProperty(ReadOnly = false)]
    public partial bool IsWeekend { get; }

    /// <summary>
    ///     假日名城
    /// </summary>
    [ObservableAsProperty(ReadOnly = false)]
    public partial string HolidayName { get; }

    /// <summary>
    ///     是否为休息日
    /// </summary>
    [ObservableAsProperty(ReadOnly = false)]
    public partial bool IsHolidayRestDay { get; }

    /// <summary>
    ///     是否正在编辑
    /// </summary>
    [Reactive]
    public partial bool IsEditing { get; set; }

    /// <summary>
    ///     编辑假日命令
    /// </summary>
    public ReactiveCommand<Unit, Unit> EditHolidayCommand;

    /// <summary>
    ///     获取假日信息交互
    /// </summary>
    public Interaction<(string, bool), (string, bool)> GetHolidayInfoInteraction;

    /// <summary>
    ///     删除假日命令
    /// </summary>
    public ReactiveCommand<Unit, Unit> RemoveHolidayCommand;

    public DayViewModel()
    {
        AllDayViewModels.Add(new WeakReference<DayViewModel>(this));

        Date = new DateInfo();
    }

    protected override void OnWhenActivated(CompositeDisposable disposable)
    {
        base.OnWhenActivated(disposable);

        var appService = Locator.Current.GetService<AppService>();

        appService
            .TimerPerMinute
            .Select(_ => Date.Date == DateTime.Today)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Do(isToday => IsCurrentDay = isToday)
            .Subscribe()
            .DisposeWith(disposable);

        this.WhenAnyValue(x => x.Date.Date)
            .Select(d => d.Date == DateTime.Today)
            .Do(isToday => IsCurrentDay = isToday)
            .Subscribe()
            .DisposeWith(disposable);

        _dayNameHelper =
            this.WhenAnyValue(x => x.Date.Date)
                .Select(d => d.Day.ToString())
                .ToProperty(this, model => model.DayName)
                .DisposeWith(disposable);

        _isWeekendHelper =
            this.WhenAnyValue(x => x.Date.Date)
                .Select(d => d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday)
                .ToProperty(this, model => model.IsWeekend)
                .DisposeWith(disposable);

        _lunarDayNameHelper =
            this.WhenAnyValue(
                    x => x.Date.LunarDayName,
                    x => x.Date.LunarMonthName,
                    x => x.Date.LunarMonthSizeFlag,
                    x => x.Date.LunarLeapMonthFlag,
                    (lunarDayName,
                        lunarMonthName,
                        lunarMonthSizeFlag,
                        lunarLeapMonthFlag) =>
                    {
                        if (lunarDayName == "初一")
                            return $"{lunarLeapMonthFlag}{lunarMonthName}{lunarMonthSizeFlag}";

                        return lunarDayName;
                    })
                .ToProperty(this, model => model.LunarDayName)
                .DisposeWith(disposable);

        _solarTermNameHelper =
            this.WhenAnyValue(
                    x => x.Date.SolarTerm,
                    x => x.Date.ShuJiuOrDogDays,
                    (solarTerm, shuJiuOrDogDays) =>
                    {
                        if (!string.IsNullOrWhiteSpace(solarTerm))
                            return solarTerm;

                        if (!string.IsNullOrWhiteSpace(shuJiuOrDogDays))
                            return shuJiuOrDogDays;

                        return "";
                    })
                .ToProperty(this, model => model.SolarTermName)
                .DisposeWith(disposable);

        _festivalNameHelper =
            this.WhenAnyValue(
                    x => x.Date.ChineseFestival,
                    x => x.Date.Festival,
                    (chineseFestival, festival) =>
                    {
                        if (!string.IsNullOrWhiteSpace(chineseFestival))
                            return chineseFestival;

                        return festival;
                    })
                .ToProperty(this, model => model.FestivalName)
                .DisposeWith(disposable);

        _isChineseFestivalHelper =
            this.WhenAnyValue(x => x.Date.ChineseFestival)
                .Select(chineseFestival => !string.IsNullOrWhiteSpace(chineseFestival))
                .ToProperty(this, model => model.IsChineseFestival)
                .DisposeWith(disposable);

        var chineseZodiacService = Locator.Current.GetService<IChineseZodiacService>();

        _chineseZodiacViewModelHelper =
            this.WhenAnyValue(x => x.Date.ChineseZodiacOfFirstMonth)
                .Select(chineseZodiacService.GetChineseZodiacViewModel)
                .ToProperty(this, model => model.ChineseZodiacViewModel)
                .DisposeWith(disposable);

        _dayWeatherImageViewModelHelper =
            this.WhenAnyValue(x => x.Forecast)
                .Select(f => f?.DayWeather?.Weather)
                .Select(w => Locator.Current.GetService<IWeatherImageService>()?.GetWeatherImageViewModel(w, false))
                .ToProperty(this, model => model.DayWeatherImageViewModel)
                .DisposeWith(disposable);

        _nightWeatherImageViewModelHelper =
            this.WhenAnyValue(x => x.Forecast)
                .Select(f => f?.NightWeather?.Weather)
                .Select(w => Locator.Current.GetService<IWeatherImageService>()?.GetWeatherImageViewModel(w, true))
                .ToProperty(this, model => model.NightWeatherImageViewModel)
                .DisposeWith(disposable);

        var holidayService = Locator.Current.GetService<IHolidayService>();

        _holidayNameHelper =
            this.WhenAnyValue(
                    x => x.Date.Date,
                    x => x.IsValid,
                    (date, _) => date)
                .Select(d =>
                {
                    var holiday = holidayService.GetHoliday(d);
                    return holiday?.Name;
                })
                .ToProperty(this, model => model.HolidayName)
                .DisposeWith(disposable);

        _isHolidayRestDayHelper =
            this.WhenAnyValue(
                    x => x.Date.Date,
                    x => x.IsValid,
                    (date, _) => date)
                .Select(d =>
                {
                    var holiday = holidayService.GetHoliday(d);
                    if (holiday == null)
                        return false;

                    return holiday.RestDates?.Contains(d.Date) ?? false;
                })
                .ToProperty(this, model => model.IsHolidayRestDay)
                .DisposeWith(disposable);

        var canEditHoliday =
            this.WhenAnyValue(
                x => x.IsEditing,
                isEditing => !isEditing);

        EditHolidayCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            try
            {
                SetIsEditing(true);

                var (holidayName, isRestDay) = await GetHolidayInfoInteraction.Handle((HolidayName, IsHolidayRestDay));

                if (string.IsNullOrWhiteSpace(holidayName))
                    return;

                if (!string.IsNullOrWhiteSpace(HolidayName) && HolidayName != holidayName)
                    holidayService.Remove(Date.Date.Year, HolidayName, Date.Date);

                holidayService.Add(Date.Date.Year, holidayName, Date.Date, isRestDay);
                IsValid = false;
                IsValid = true;
            }
            finally
            {
                SetIsEditing(false);
            }
        }, canEditHoliday);

        RemoveHolidayCommand = ReactiveCommand.Create(() =>
        {
            holidayService.Remove(Date.Date.Year, HolidayName, Date.Date);
            IsValid = false;
            IsValid = true;
        }, canEditHoliday);

        GetHolidayInfoInteraction = new Interaction<(string, bool), (string, bool)>();
    }

    ~DayViewModel()
    {
        AllDayViewModels.RemoveAll(d =>
        {
            if (d.TryGetTarget(out var day))
                return day == this;

            return false;
        });
    }

    private static void SetIsEditing(bool isEditing)
    {
        foreach (var dayViewModel in AllDayViewModels)
            if (dayViewModel.TryGetTarget(out var day))
                day.IsEditing = isEditing;
    }

    public override string ToString()
    {
        return Date?.Date.ToString("yyyy-MM-dd");
    }
}