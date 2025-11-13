using System;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Windows;
using ReactiveUI;
using Splat;
using WeatherCalendar.Services;
using WeatherCalendar.Themes;

namespace WeatherCalendar.Views;

/// <summary>
///     WorkTimerView.xaml 的交互逻辑
/// </summary>
public partial class WorkTimerView
{
    public WorkTimerView()
    {
        InitializeComponent();

        this.WhenActivated(WhenActivated);
    }

    private void WhenActivated(CompositeDisposable disposable)
    {
        this.OneWayBind(
                ViewModel,
                model => model.CountdownType,
                view => view.TimerTypeTextBlock.Text,
                type =>
                {
                    return type switch
                    {
                        WorkCountdownType.BeforeWork => "上班",
                        WorkCountdownType.BeforeOffWork => "下班",
                        _ => ""
                    };
                })
            .DisposeWith(disposable);

        this.WhenAnyValue(x => x.ViewModel.CountdownTime)
            .Do(time =>
            {
                var totalMinutes = time.TotalMinutes;
                if (totalMinutes > 1)
                {
                    totalMinutes++;
                    HourTextBlock.Text = $"{Math.Floor(totalMinutes / 60)}";
                    MinuteTextBlock.Text = $"{Math.Floor(totalMinutes % 60)}";
                    Const3TextBlock.Text = "小时";
                    Const4TextBlock.Visibility = Visibility.Visible;
                    MinuteTextBlock.Visibility = Visibility.Visible;
                }
                else
                {
                    HourTextBlock.Text = $"{Math.Floor(time.TotalSeconds)}";
                    Const3TextBlock.Text = "秒";
                    Const4TextBlock.Visibility = Visibility.Collapsed;
                    MinuteTextBlock.Visibility = Visibility.Collapsed;
                }

                var theme = Locator.Current.GetService<ITheme>();
                Const1TextBlock.Foreground = theme.WorkTimerNormalForeground;
                Const2TextBlock.Foreground = theme.WorkTimerNormalForeground;
                Const3TextBlock.Foreground = theme.WorkTimerNormalForeground;
                Const4TextBlock.Foreground = theme.WorkTimerNormalForeground;

                TimerTypeTextBlock.Foreground = theme.WorkTimerTimeForeground;
                HourTextBlock.Foreground = theme.WorkTimerTimeForeground;
                MinuteTextBlock.Foreground = theme.WorkTimerTimeForeground;
            })
            .Subscribe()
            .DisposeWith(disposable);

        this.WhenAnyValue(
                x => x.ViewModel.IsVisible,
                x => x.ViewModel.CountdownType,
                (isVisible, type) =>
                    isVisible && type != WorkCountdownType.None
                        ? Visibility.Visible
                        : Visibility.Collapsed)
            .BindTo(this, view => view.Visibility)
            .DisposeWith(disposable);
    }
}