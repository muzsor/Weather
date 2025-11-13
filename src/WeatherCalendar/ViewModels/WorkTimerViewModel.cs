using System;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;
using WeatherCalendar.Services;

// ReSharper disable UnassignedGetOnlyAutoProperty

namespace WeatherCalendar.ViewModels;

public partial class WorkTimerViewModel : ReactiveBase
{
    /// <summary>
    ///     倒计时类型
    /// </summary>
    [ObservableAsProperty(ReadOnly = false)]
    public partial WorkCountdownType CountdownType { get; }

    /// <summary>
    ///     倒计时
    /// </summary>
    [ObservableAsProperty(ReadOnly = false)]
    public partial TimeSpan CountdownTime { get; }

    /// <summary>
    ///     是否显示
    /// </summary>
    [ObservableAsProperty(ReadOnly = false)]
    public partial bool IsVisible { get; }

    protected override void OnWhenActivated(CompositeDisposable disposable)
    {
        base.OnWhenActivated(disposable);

        var workTimerService = Locator.Current.GetService<WorkTimerService>();

        _isVisibleHelper =
            workTimerService
                .WhenAnyValue(x => x.IsVisible)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, model => model.IsVisible)
                .DisposeWith(disposable);

        _countdownTypeHelper =
            workTimerService
                .WhenAnyValue(x => x.CountdownType)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, model => model.CountdownType)
                .DisposeWith(disposable);

        _countdownTimeHelper =
            workTimerService
                .WhenAnyValue(x => x.CountdownTime)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, model => model.CountdownTime)
                .DisposeWith(disposable);
    }
}