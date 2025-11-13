using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

// ReSharper disable UnassignedGetOnlyAutoProperty

namespace WeatherCalendar.ViewModels;

public partial class ChineseZodiacFontViewModel : ReactiveBase
{
    [Reactive]
    public partial string ChineseZodiac { get; set; }

    [ObservableAsProperty(ReadOnly = false)]
    public partial string Text { get; }

    protected override void OnWhenActivated(CompositeDisposable disposable)
    {
        base.OnWhenActivated(disposable);

        _textHelper =
            this.WhenAnyValue(x => x.ChineseZodiac)
                .Select(GetText)
                .ToProperty(this, model => model.Text)
                .DisposeWith(disposable);
    }

    private string GetText(string chineseZodiac)
    {
        return chineseZodiac switch
        {
            "鼠" => "\ue663",
            "牛" => "\ue66a",
            "虎" => "\ue668",
            "兔" => "\ue669",
            "龙" => "\ue661",
            "蛇" => "\ue662",
            "马" => "\ue665",
            "羊" => "\ue66b",
            "猴" => "\ue660",
            "鸡" => "\ue666",
            "狗" => "\ue667",
            "猪" => "\ue664",
            _ => ""
        };
    }
}