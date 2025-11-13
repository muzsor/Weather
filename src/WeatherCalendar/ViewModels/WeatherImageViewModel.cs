using ReactiveUI.SourceGenerators;

namespace WeatherCalendar.ViewModels;

public partial class WeatherImageViewModel : ReactiveBase
{
    [Reactive]
    public partial string ImageFile { get; set; }
}