using System;
using System.Linq;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;

// ReSharper disable UnassignedGetOnlyAutoProperty

namespace WeatherCalendar.Services;

/// <summary>
///     工作倒计时类型
/// </summary>
public enum WorkCountdownType
{
    /// <summary>
    ///     无
    /// </summary>
    None,

    /// <summary>
    ///     工作前
    /// </summary>
    BeforeWork,

    /// <summary>
    ///     下班前
    /// </summary>
    BeforeOffWork
}

/// <summary>
///     工作计时器服务
///     用来为上下班倒计时的服务
/// </summary>
public partial class WorkTimerService : ReactiveBase
{
    /// <summary>
    ///     是否显示
    /// </summary>
    [Reactive]
    public partial bool IsVisible { get; set; }

    /// <summary>
    ///     开始时间
    /// </summary>
    [Reactive]
    public partial TimeSpan StartTime { get; set; }

    /// <summary>
    ///     结束时间
    /// </summary>
    [Reactive]
    public partial TimeSpan EndTime { get; set; }

    /// <summary>
    ///     倒计时类型
    /// </summary>
    [ObservableAsProperty]
    public partial WorkCountdownType CountdownType { get; }

    /// <summary>
    ///     倒计时
    /// </summary>
    [ObservableAsProperty]
    public partial TimeSpan CountdownTime { get; }

    public WorkTimerService()
    {
        var appService = Locator.Current.GetService<AppService>();
        var holidayService = Locator.Current.GetService<IHolidayService>();

        var timer =
            appService
                .TimerPerSecond
                .Select(time =>
                {
                    if (!IsVisible)
                        return (WorkCountdownType.None, TimeSpan.Zero);

                    var holiday = holidayService.GetHoliday(time);

                    // 假期休息日
                    if (holiday?.RestDates != null
                        && holiday.RestDates.Any(d => d.Date == time.Date))
                        return (WorkCountdownType.None, TimeSpan.Zero);

                    // 假期工作日
                    if (holiday?.WorkDates != null
                        && holiday.WorkDates.Any(d => d.Date == time.Date))
                        return GetCountdownInfo(time.TimeOfDay);

                    // 周末
                    if (time.DayOfWeek == DayOfWeek.Saturday
                        || time.DayOfWeek == DayOfWeek.Sunday)
                        return (WorkCountdownType.None, TimeSpan.Zero);

                    // 工作日
                    return GetCountdownInfo(time.TimeOfDay);
                });

        _countdownTypeHelper =
            timer.Select(d => d.Item1)
                .ToProperty(this, service => service.CountdownType);

        _countdownTimeHelper =
            timer.Select(d => d.Item2)
                .ToProperty(this, service => service.CountdownTime);
    }

    private (WorkCountdownType, TimeSpan) GetCountdownInfo(TimeSpan currentTime)
    {
        if (currentTime <= StartTime)
            return (WorkCountdownType.BeforeWork, StartTime - currentTime);

        if (currentTime <= EndTime)
            return (WorkCountdownType.BeforeOffWork, EndTime - currentTime);

        return (WorkCountdownType.None, TimeSpan.Zero);
    }
}