using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;
using WeatherCalendar.Models;

// ReSharper disable UnassignedGetOnlyAutoProperty

namespace WeatherCalendar.Services;

public partial class SystemInfoService : ReactiveBase
{
    private static readonly PerformanceCounter CpuLoadCounter = new("Processor", "% Processor Time", "_Total");

    /// <summary>
    ///     CPU 使用率
    /// </summary>
    [ObservableAsProperty]
    public partial float CpuLoad { get; }

    /// <summary>
    ///     物理内存
    /// </summary>
    [Reactive]
    public partial long PhysicalMemory { get; set; }

    /// <summary>
    ///     可用内存
    /// </summary>
    [ObservableAsProperty]
    public partial long AvailableMemory { get; }

    /// <summary>
    ///     内存使用率
    /// </summary>
    [ObservableAsProperty]
    public partial float MemoryLoad { get; }

    /// <summary>
    ///     网络速度
    /// </summary>
    [ObservableAsProperty]
    public partial NetWorkInfo NetWorkInfo { get; }

    private static IEnumerable<NetworkInterface> NetworkInterfaces =>
        NetworkInterface.GetAllNetworkInterfaces()
            .Where(n => n.OperationalStatus == OperationalStatus.Up)
            .DistinctBy(n => n.GetPhysicalAddress())
            .ToList();

    private long LastTotalSend { get; set; }

    private long LastTotalReceived { get; set; }

    public SystemInfoService()
    {
        var appService = Locator.Current.GetService<AppService>();

        var mc = new ManagementClass("Win32_ComputerSystem");
        var moc = mc.GetInstances();
        foreach (var mo in moc)
            if (mo["TotalPhysicalMemory"] != null)
                PhysicalMemory = long.Parse(mo["TotalPhysicalMemory"].ToString()!);

        _netWorkInfoHelper =
            appService
                .TimerPerSecond
                .Select(_ => Update())
                .ToProperty(this, monitor => monitor.NetWorkInfo);

        _cpuLoadHelper =
            appService
                .TimerPerSecond
                .Select(_ => CpuLoadCounter.NextValue())
                .ToProperty(this, service => service.CpuLoad);

        _availableMemoryHelper =
            appService
                .TimerPerSecond
                .Select(_ =>
                {
                    long availableBytes = 0;
                    try
                    {
                        var mos = new ManagementClass("Win32_OperatingSystem");
                        foreach (var mo in mos.GetInstances())
                            availableBytes = 1024 * long.Parse(mo["FreePhysicalMemory"]!.ToString() ?? "0");
                    }
                    catch
                    {
                        //
                    }

                    return availableBytes;
                })
                .ToProperty(this, service => service.AvailableMemory);

        _memoryLoadHelper =
            this.WhenAnyValue(x => x.AvailableMemory)
                .Select(available => (PhysicalMemory - available) / (float)PhysicalMemory * 100)
                .ToProperty(this, service => service.MemoryLoad);
    }

    private NetWorkInfo Update()
    {
        long tempSent = 0;
        long tempReceived = 0;
        foreach (var networkInterface in NetworkInterfaces)
        {
            tempSent += networkInterface.GetIPStatistics().BytesSent / 1024;
            tempReceived += networkInterface.GetIPStatistics().BytesReceived / 1024;
        }

        LastTotalSend = NetWorkInfo?.TotalSend ?? 0;
        LastTotalReceived = NetWorkInfo?.TotalReceived ?? 0;

        if (NetWorkInfo == null)
            return new NetWorkInfo(0, 0, tempSent, tempReceived);

        var totalSend = tempSent;
        var totalReceived = tempReceived;
        var sentSpeed = totalSend - LastTotalSend;
        var receivedSpeed = totalReceived - LastTotalReceived;

        return new NetWorkInfo(sentSpeed, receivedSpeed, totalSend, totalReceived);
    }
}