using ReactiveUI.SourceGenerators;

namespace WeatherCalendar.ViewModels;

public partial class WeatherFontViewModel : ReactiveBase
{
    [Reactive]
    public partial string WeatherText { get; set; }
}