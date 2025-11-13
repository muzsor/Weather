using System.Reactive.Disposables;
using ReactiveUI;

namespace WeatherCalendar;

public class ReactiveBase : ReactiveObject, IActivatableViewModel
{
    public ReactiveBase()
    {
        this.WhenActivated(OnWhenActivated);
    }

    public ViewModelActivator Activator { get; } = new();

    protected virtual void OnWhenActivated(CompositeDisposable disposable)
    {
    }
}