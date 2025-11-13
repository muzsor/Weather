using System;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Windows.Input;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using Splat;
using WeatherCalendar.Services;
using WeatherCalendar.ViewModels;

namespace WeatherCalendar.Views;

/// <summary>
///     CalendarWindow.xaml 的交互逻辑
/// </summary>
public partial class CalendarWindow
{
    private bool IsChangingCity { get; set; }

    public CalendarWindow()
    {
        InitializeComponent();

        ViewModel = new CalendarWindowViewModel();

        this.WhenActivated(WhenActivated);
    }

    private void WhenActivated(CompositeDisposable disposable)
    {
        this.OneWayBind(
                ViewModel,
                model => model.Years,
                view => view.YearsComboBox.ItemsSource)
            .DisposeWith(disposable);

        this.OneWayBind(
                ViewModel,
                model => model.Months,
                view => view.MonthsComboBox.ItemsSource)
            .DisposeWith(disposable);

        this.OneWayBind(
                ViewModel,
                model => model.Calendar,
                view => view.CalendarViewModelViewHost.ViewModel)
            .DisposeWith(disposable);

        this.Bind(
                ViewModel,
                model => model.SelectedYear,
                view => view.YearsComboBox.SelectedValue)
            .DisposeWith(disposable);

        this.Bind(
                ViewModel,
                model => model.SelectedMonth,
                view => view.MonthsComboBox.SelectedValue)
            .DisposeWith(disposable);

        this.OneWayBind(
                ViewModel,
                model => model.Forecast,
                view => view.UpdateTimeTextBlock.Text,
                forecast =>
                {
                    var time = forecast?.Status?.UpdateTime;
                    return string.IsNullOrWhiteSpace(time) ? "" : $"{time}更新";
                })
            .DisposeWith(disposable);

        this.OneWayBind(
                ViewModel,
                model => model.Forecast,
                view => view.CityButton.Content,
                forecast => forecast?.Status?.City)
            .DisposeWith(disposable);

        UpdateButton
            .Events()
            .Click
            .Do(_ =>
            {
                var weatherService = Locator.Current.GetService<WeatherService>();
                weatherService.UpdateWeather();
            })
            .Subscribe()
            .DisposeWith(disposable);

        CityButton
            .Events()
            .Click
            .Do(_ => ChangeWeatherCity())
            .Subscribe()
            .DisposeWith(disposable);

        PinButton
            .Events()
            .Click
            .Do(_ => Topmost = !Topmost)
            .Subscribe()
            .DisposeWith(disposable);

        this.BindCommand(
                ViewModel!,
                model => model.CurrentMonthCommand,
                view => view.TodayButton)
            .DisposeWith(disposable);

        this.BindCommand(
                ViewModel!,
                model => model.LastMonthCommand,
                view => view.LastMonthButton)
            .DisposeWith(disposable);

        this.BindCommand(
                ViewModel!,
                model => model.NextMonthCommand,
                view => view.NextMonthButton)
            .DisposeWith(disposable);

        TitleBorder
            .Events()
            .MouseLeftButtonDown
            .Do(_ => DragMove())
            .Subscribe()
            .DisposeWith(disposable);

        CloseButton
            .Events()
            .Click
            .Do(_ => Close())
            .Subscribe()
            .DisposeWith(disposable);

        YearsComboBox
            .Events()
            .SelectionChanged
            .Do(_ => MoveFocus(new TraversalRequest(FocusNavigationDirection.Next)))
            .Subscribe()
            .DisposeWith(disposable);

        MonthsComboBox
            .Events()
            .SelectionChanged
            .Do(_ => MoveFocus(new TraversalRequest(FocusNavigationDirection.Next)))
            .Subscribe()
            .DisposeWith(disposable);
    }

    private void ChangeWeatherCity()
    {
        if (IsChangingCity)
            return;

        IsChangingCity = true;
        var cityWindow = new SelectCityWindow();
        cityWindow.Closed += (_, _) => IsChangingCity = false;
        cityWindow.Show();
    }
}