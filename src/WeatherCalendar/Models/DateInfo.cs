using System;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;
using WeatherCalendar.Services;

// ReSharper disable UnassignedGetOnlyAutoProperty

namespace WeatherCalendar.Models;

public partial class DateInfo : ReactiveBase
{
    /// <summary>
    ///     公历日期
    /// </summary>
    [Reactive]
    public partial DateTime Date { get; set; }

    /// <summary>
    ///     干支年（正月）
    /// </summary>
    [ObservableAsProperty]
    public partial string StemsAndBranchesYearNameOfFirstMonth { get; }

    /// <summary>
    ///     干支年（立春）
    /// </summary>
    [ObservableAsProperty]
    public partial string StemsAndBranchesYearNameOfSpringBegins { get; }

    /// <summary>
    ///     生肖（正月）
    /// </summary>
    [ObservableAsProperty]
    public partial string ChineseZodiacOfFirstMonth { get; }

    /// <summary>
    ///     生肖（立春）
    /// </summary>
    [ObservableAsProperty]
    public partial string ChineseZodiacOfSpringBegins { get; }

    /// <summary>
    ///     农历月
    /// </summary>
    [ObservableAsProperty]
    public partial string LunarMonthName { get; }

    /// <summary>
    ///     干支月
    /// </summary>
    [ObservableAsProperty]
    public partial string StemsAndBranchesMonthName { get; }

    /// <summary>
    ///     农历闰月（‘闰’或空）
    /// </summary>
    [ObservableAsProperty]
    public partial string LunarLeapMonthFlag { get; }

    /// <summary>
    ///     农历月大小
    /// </summary>
    [ObservableAsProperty]
    public partial string LunarMonthSizeFlag { get; }

    /// <summary>
    ///     农历月信息
    /// </summary>
    [ObservableAsProperty]
    public partial string LunarMonthInfo { get; }

    /// <summary>
    ///     农历日
    /// </summary>
    [ObservableAsProperty]
    public partial string LunarDayName { get; }

    /// <summary>
    ///     干支日
    /// </summary>
    [ObservableAsProperty]
    public partial string StemsAndBranchesDayName { get; }

    /// <summary>
    ///     节气
    /// </summary>
    [ObservableAsProperty]
    public partial string SolarTerm { get; }

    /// <summary>
    ///     三九或三伏
    /// </summary>
    [ObservableAsProperty]
    public partial string ShuJiuOrDogDays { get; }

    /// <summary>
    ///     数九详情
    /// </summary>
    [ObservableAsProperty]
    public partial string ShuJiuDetail { get; }

    /// <summary>
    ///     三伏详情
    /// </summary>
    [ObservableAsProperty]
    public partial string DogDaysDetail { get; }

    /// <summary>
    ///     中国节假日
    /// </summary>
    [ObservableAsProperty]
    public partial string ChineseFestival { get; }

    /// <summary>
    ///     节假日
    /// </summary>
    [ObservableAsProperty]
    public partial string Festival { get; }

    public DateInfo()
    {
        var calendarService = Locator.Current.GetService<CalendarService>();

        _stemsAndBranchesYearNameOfFirstMonthHelper =
            this.WhenAnyValue(x => x.Date)
                .Select(calendarService.GetStemsAndBranchesYearNameOfFirstMonth)
                .ToProperty(this, info => info.StemsAndBranchesYearNameOfFirstMonth);

        _stemsAndBranchesYearNameOfSpringBeginsHelper =
            this.WhenAnyValue(x => x.Date)
                .Select(calendarService.GetStemsAndBranchesYearNameOfSpringBegins)
                .ToProperty(this, info => info.StemsAndBranchesYearNameOfSpringBegins);

        _chineseZodiacOfFirstMonthHelper =
            this.WhenAnyValue(x => x.Date)
                .Select(calendarService.GetChineseZodiacOfFirstMonth)
                .ToProperty(this, info => info.ChineseZodiacOfFirstMonth);

        _chineseZodiacOfSpringBeginsHelper =
            this.WhenAnyValue(x => x.Date)
                .Select(calendarService.GetChineseZodiacOfSpringBegins)
                .ToProperty(this, info => info.ChineseZodiacOfSpringBegins);

        _lunarMonthNameHelper =
            this.WhenAnyValue(x => x.Date)
                .Select(calendarService.GetLunarMonthName)
                .ToProperty(this, info => info.LunarMonthName);

        _stemsAndBranchesMonthNameHelper =
            this.WhenAnyValue(x => x.Date)
                .Select(calendarService.GetStemsAndBranchesMonthName)
                .ToProperty(this, info => info.StemsAndBranchesMonthName);

        _lunarLeapMonthFlagHelper =
            this.WhenAnyValue(x => x.Date)
                .Select(calendarService.GetLunarLeapMonthFlag)
                .ToProperty(this, info => info.LunarLeapMonthFlag);

        _lunarMonthSizeFlagHelper =
            this.WhenAnyValue(x => x.Date)
                .Select(calendarService.GetLunarMonthSizeFlag)
                .ToProperty(this, info => info.LunarMonthSizeFlag);

        _lunarMonthInfoHelper =
            this.WhenAnyValue(x => x.Date)
                .Select(calendarService.GetLunarMonthInfo)
                .ToProperty(this, info => info.LunarMonthInfo);

        _lunarDayNameHelper =
            this.WhenAnyValue(x => x.Date)
                .Select(calendarService.GetLunarDayName)
                .ToProperty(this, info => info.LunarDayName);

        _stemsAndBranchesDayNameHelper =
            this.WhenAnyValue(x => x.Date)
                .Select(calendarService.GetStemsAndBranchesDayName)
                .ToProperty(this, info => info.StemsAndBranchesDayName);

        _solarTermHelper =
            this.WhenAnyValue(x => x.Date)
                .Select(calendarService.GetSolarTerm)
                .ToProperty(this, info => info.SolarTerm);

        _shuJiuOrDogDaysHelper =
            this.WhenAnyValue(x => x.Date)
                .Select(calendarService.GetShuJiuOrDogDays)
                .ToProperty(this, info => info.ShuJiuOrDogDays);

        _shuJiuDetailHelper =
            this.WhenAnyValue(x => x.Date)
                .Select(calendarService.GetShuJiuDetail)
                .ToProperty(this, info => info.ShuJiuDetail);

        _dogDaysDetailHelper =
            this.WhenAnyValue(x => x.Date)
                .Select(calendarService.GetDogDaysDetail)
                .ToProperty(this, info => info.DogDaysDetail);

        var festivalService = Locator.Current.GetService<FestivalService>();

        _chineseFestivalHelper =
            this.WhenAnyValue(x => x.Date)
                .Select(festivalService.GetLunarFestival)
                .ToProperty(this, info => info.ChineseFestival);

        _festivalHelper =
            this.WhenAnyValue(x => x.Date)
                .Select(festivalService.GetFestival)
                .ToProperty(this, info => info.Festival);
    }
}