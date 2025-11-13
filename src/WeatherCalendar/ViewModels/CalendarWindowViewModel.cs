using System;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;
using Weather;
using WeatherCalendar.Services;

// ReSharper disable UnassignedGetOnlyAutoProperty

namespace WeatherCalendar.ViewModels;

public partial class CalendarWindowViewModel : CalendarBaseViewModel
{
    /// <summary>
    ///     日历
    /// </summary>
    public CalendarViewModel Calendar { get; }

    /// <summary>
    ///     年
    /// </summary>
    public int[] Years { get; }

    /// <summary>
    ///     月
    /// </summary>
    public int[] Months { get; }

    /// <summary>
    ///     选中的年
    /// </summary>
    [Reactive]
    public partial int SelectedYear { get; set; }

    /// <summary>
    ///     选中的月
    /// </summary>
    [Reactive]
    public partial int SelectedMonth { get; set; }

    /// <summary>
    ///     天气预报
    /// </summary>
    [ObservableAsProperty(ReadOnly = false)]
    public partial WeatherForecast Forecast { get; }

    /// <summary>
    ///     是否置顶
    /// </summary>
    [Reactive]
    public partial bool IsTopmost { get; set; }

    public CalendarWindowViewModel()
    {
        Calendar = new CalendarViewModel();

        GotoMonthCommand = Calendar.GotoMonthCommand;
        CurrentMonthCommand = Calendar.CurrentMonthCommand;
        LastMonthCommand = Calendar.LastMonthCommand;
        NextMonthCommand = Calendar.NextMonthCommand;

        Years = new int[199];
        for (var i = 0; i < Years.Length; i++) Years[i] = 1902 + i;

        Months = new int[12];
        for (var i = 0; i < Months.Length; i++) Months[i] = i + 1;

        SelectedYear = Calendar.CurrentMonth.Year;
        SelectedMonth = Calendar.CurrentMonth.Month;

        this.WhenAnyValue(
                x => x.SelectedYear,
                x => x.SelectedMonth,
                (year, month) => new DateTime(year, month, 1))
            .InvokeCommand(this, model => model.GotoMonthCommand);
    }

    protected override void OnWhenActivated(CompositeDisposable disposable)
    {
        base.OnWhenActivated(disposable);

        var weatherService = Locator.Current.GetService<WeatherService>();

        _forecastHelper =
            weatherService
                .WhenAnyValue(x => x.Forecast)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, model => model.Forecast)
                .DisposeWith(disposable);

        Calendar
            .WhenAnyValue(x => x.CurrentMonth)
            .Do(date =>
            {
                SelectedYear = date.Year;
                SelectedMonth = date.Month;
            })
            .Subscribe()
            .DisposeWith(disposable);
    }
}