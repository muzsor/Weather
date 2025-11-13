using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace WeatherCalendar.ViewModels;

public partial class EditHolidayWindowViewModel : ReactiveBase
{
    /// <summary>
    ///     假日名称
    /// </summary>
    [Reactive]
    public partial string HolidayName { get; set; }

    /// <summary>
    ///     是否为休息日
    /// </summary>
    [Reactive]
    public partial bool IsRestDay { get; set; }

    /// <summary>
    ///     是否已确定
    /// </summary>
    [Reactive]
    public partial bool IsConfirmed { get; set; }

    /// <summary>
    ///     确定命令
    /// </summary>
    public ReactiveCommand<Unit, Unit> ConfirmCommand;

    /// <summary>
    ///     提交完成交互
    /// </summary>
    public Interaction<(string, bool), Unit> ConfirmedInteraction;

    public EditHolidayWindowViewModel()
    {
        IsConfirmed = false;

        var canConfirmCommandExecute =
            this.WhenAnyValue(x => x.HolidayName)
                .Select(n => !string.IsNullOrWhiteSpace(n));

        ConfirmCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            IsConfirmed = true;
            await ConfirmedInteraction.Handle((HolidayName, IsRestDay));
        }, canConfirmCommandExecute);

        ConfirmedInteraction = new Interaction<(string, bool), Unit>();
    }
}