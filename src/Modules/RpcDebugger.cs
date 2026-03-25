using Microsoft.Extensions.Logging;
using SFSharp;
using SFSharp.Interop.RakNet.Packets.Enum;
using System.Collections.Concurrent;

[SFModule("rpc-debugger", "RpcDebugger", Category = "Debug", Description = "Captures incoming and outgoing RPC/packet traffic with lightweight decoding.", ExecutionModel = ModuleExecutionModel.MainThread, Order = 60)]
public class RpcDebugger : SFModuleBase
{
    private const int MaxEntries = 200;
    private const int EntriesPerPage = 20;

    private readonly ConcurrentQueue<NetLogEntry> _log = new();
    private static readonly SFColor SuccessColor = SFColors.Green;
    private static readonly SFColor DangerColor = SFColors.Red;
    private static readonly SFColor IncomingColor = SFColor.FromHex("00AAFF");
    private static readonly SFColor OutgoingColor = SFColor.FromHex("FFAA00");
    private static readonly SFColor RpcColor = SFColor.FromHex("CCCCFF");
    private static readonly SFColor PacketColor = SFColor.FromHex("FFCCAA");
    private static readonly SFColor MutedColor = SFColors.Gray;
    private static readonly SFColor SoftMutedColor = SFColor.FromHex("888888");
    private static readonly SFColor NavColor = SFColor.FromHex("55AAFF");
    private static readonly SFColor HeaderColor = SFColors.Cyan | SFColors.Blue;
    private static readonly SFColor TitleColor = SFColors.Yellow | SFColors.Orange;
    private static readonly SFColor ValueColor = SFColors.White;
    private volatile bool _captureEnabled;
    private volatile bool _captureIncoming = true;
    private volatile bool _captureOutgoing = true;
    private volatile bool _captureRpc = true;
    private volatile bool _capturePackets = true;
    private int _totalIncomingRpc;
    private int _totalOutgoingRpc;
    private int _totalIncomingPacket;
    private int _totalOutgoingPacket;
    private enum Direction { Incoming, Outgoing }
    private enum MessageKind { Rpc, Packet }

    private readonly record struct NetLogEntry(
        Direction Direction,
        MessageKind Kind,
        int Id,
        string? Name,
        string? Detail,
        int DataBitLength,
        long Timestamp);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        UpdateRuntimeFlags();
        using IDisposable command = Context.RegisterChatCommand("rpcd", OnCommand);

        List<IDisposable> subscriptions = new();
        try
        {
            foreach (ERpcId rpcId in Enum.GetValues<ERpcId>())
            {
                subscriptions.Add(Context.RegisterDisposable(SF.Rpc.Subscribe(rpcId, args => OnIncomingRpc(args))));
                subscriptions.Add(Context.RegisterDisposable(SF.Rpc.SubscribeOutgoing(rpcId, args => OnOutgoingRpc(args))));
            }

            foreach (EPacketId packetId in Enum.GetValues<EPacketId>())
            {
                subscriptions.Add(Context.RegisterDisposable(SF.Packets.SubscribeIncoming(packetId, args => OnIncomingPacket(args))));
                subscriptions.Add(Context.RegisterDisposable(SF.Packets.SubscribeOutgoing(packetId, args => OnOutgoingPacket(args))));
            }

            Context.SetDetail("subscriptions", subscriptions.Count.ToString());

            while (!cancellationToken.IsCancellationRequested)
            {
                using (ModuleLoopScope _ = Context.TrackLoop("dispatcher-idle"))
                {
                    Context.Heartbeat("waiting network events");
                }

                await Task.Yield();
            }
        }
        finally
        {
            foreach (IDisposable sub in subscriptions)
            {
                sub.Dispose();
            }
        }
    }

    private void OnIncomingRpc(IncomingRpcArgs args)
    {
        Interlocked.Increment(ref _totalIncomingRpc);
        Context.IncrementCounter("rpc.in.total");
        if (!_captureEnabled || !_captureIncoming || !_captureRpc)
        {
            return;
        }

        string? name = Enum.IsDefined((ERpcId)args.ERpcId) ? ((ERpcId)args.ERpcId).ToString() : null;
        string? detail = $"direction=Incoming rpcId={args.ERpcId}";
        Enqueue(new NetLogEntry(Direction.Incoming, MessageKind.Rpc, args.ERpcId, name, detail, args.DataBitLength, Environment.TickCount64));
        Context.Heartbeat($"rpc-in:{args.ERpcId}");
    }

    private void OnOutgoingRpc(OutgoingRpcArgs args)
    {
        Interlocked.Increment(ref _totalOutgoingRpc);
        Context.IncrementCounter("rpc.out.total");
        if (!_captureEnabled || !_captureOutgoing || !_captureRpc)
        {
            return;
        }

        string? name = Enum.IsDefined((ERpcId)args.ERpcId) ? ((ERpcId)args.ERpcId).ToString() : null;
        string? detail = $"direction=Outgoing rpcId={args.ERpcId}";
        Enqueue(new NetLogEntry(Direction.Outgoing, MessageKind.Rpc, args.ERpcId, name, detail, args.DataBitLength, Environment.TickCount64));
        Context.Heartbeat($"rpc-out:{args.ERpcId}");
    }

    private void OnIncomingPacket(IncomingPacketArgs args)
    {
        Interlocked.Increment(ref _totalIncomingPacket);
        Context.IncrementCounter("packet.in.total");
        if (!_captureEnabled || !_captureIncoming || !_capturePackets)
        {
            return;
        }

        (string? name, string? detail) = DecodeIncomingPacket(args);
        Enqueue(new NetLogEntry(Direction.Incoming, MessageKind.Packet, args.EPacketId, name, detail, args.DataBitLength, Environment.TickCount64));
        Context.Heartbeat($"pkt-in:{args.EPacketId}");
    }

    private void OnOutgoingPacket(OutgoingPacketArgs args)
    {
        Interlocked.Increment(ref _totalOutgoingPacket);
        Context.IncrementCounter("packet.out.total");
        if (!_captureEnabled || !_captureOutgoing || !_capturePackets)
        {
            return;
        }

        (string? name, string? detail) = DecodeOutgoingPacket(args);
        Enqueue(new NetLogEntry(Direction.Outgoing, MessageKind.Packet, args.EPacketId, name, detail, args.DataBitLength, Environment.TickCount64));
        Context.Heartbeat($"pkt-out:{args.EPacketId}");
    }

    private static (string? Name, string? Detail) DecodeIncomingPacket(IncomingPacketArgs args)
    {
        if (SF.PacketParsers.TryParseIncoming(args, out PacketParseResult result) && result.Packet is IParsedIncomingPacket packet)
        {
            return FormatParsedPacket(packet, args.EPacketId);
        }

        return FormatPacketParseFailure(args.EPacketId, result, TryReadArizonaSubId(args));
    }

    private static (string? Name, string? Detail) DecodeOutgoingPacket(OutgoingPacketArgs args)
    {
        if (SF.PacketParsers.TryParseOutgoing(args, out PacketParseResult result) && result.Packet is IParsedOutgoingPacket packet)
        {
            return FormatParsedPacket(packet, args.EPacketId);
        }

        return FormatPacketParseFailure(args.EPacketId, result, TryReadArizonaSubId(args));
    }

    private static (string? Name, string? Detail) FormatParsedPacket(IParsedPacket packet, int rawPacketId)
    {
        if (packet is IParsedArizonaPacket arizonaPacket)
        {
            EPacketId packetId = (EPacketId)rawPacketId;
            string transport = packetId == EPacketId.ArizonaCefEx ? "Arizona221" : "Arizona220";
            string name = $"{transport}:{packet.Name}";
            string detail = packet.Detail is { Length: > 0 }
                ? $"subId={arizonaPacket.SubId} {packet.Detail}"
                : $"subId={arizonaPacket.SubId}";
            return (name, detail);
        }

        return (packet.Name, packet.Detail);
    }

    private static (string? Name, string? Detail) FormatPacketParseFailure(int rawPacketId, PacketParseResult result, int? arizonaSubId)
    {
        PacketParseFailureReason reason = result.FailureReason;
        string? fallbackName = Enum.IsDefined((EPacketId)rawPacketId) ? ((EPacketId)rawPacketId).ToString() : null;
        if (arizonaSubId is int subId)
        {
            EPacketId packetId = (EPacketId)rawPacketId;
            string transport = packetId == EPacketId.ArizonaCefEx ? "Arizona221" : "Arizona220";
            string? detail = reason is PacketParseFailureReason.Unsupported or PacketParseFailureReason.None
                ? $"subId={subId}"
                : $"subId={subId} parse={reason}" + (string.IsNullOrWhiteSpace(result.ErrorMessage) ? string.Empty : $" error={result.ErrorMessage}");
            return ($"{transport}:{fallbackName}", detail);
        }

        string? failure = reason is PacketParseFailureReason.Unsupported or PacketParseFailureReason.None
            ? null
            : $"parse={reason}" + (string.IsNullOrWhiteSpace(result.ErrorMessage) ? string.Empty : $" error={result.ErrorMessage}");
        return (fallbackName, failure);
    }

    private static int? TryReadArizonaSubId(IncomingPacketArgs args)
    {
        EPacketId packetId = (EPacketId)args.EPacketId;
        if (packetId is not (EPacketId.ArizonaCef or EPacketId.ArizonaCefEx))
        {
            return null;
        }

        try
        {
            BitStreamReader reader = args.CreateReader();
            reader.SkipBytes(1);
            return packetId == EPacketId.ArizonaCef
                ? ArizonaPacket.ReadSubId220(ref reader)
                : ArizonaPacket.ReadSubId221(ref reader);
        }
        catch
        {
            return null;
        }
    }

    private static int? TryReadArizonaSubId(OutgoingPacketArgs args)
    {
        EPacketId packetId = (EPacketId)args.EPacketId;
        if (packetId is not (EPacketId.ArizonaCef or EPacketId.ArizonaCefEx))
        {
            return null;
        }

        try
        {
            BitStreamReader reader = args.CreateReader();
            reader.SkipBytes(1);
            return packetId == EPacketId.ArizonaCef
                ? ArizonaPacket.ReadSubId220(ref reader)
                : ArizonaPacket.ReadSubId221(ref reader);
        }
        catch
        {
            return null;
        }
    }

    private void Enqueue(NetLogEntry entry)
    {
        _log.Enqueue(entry);
        Context.SetDetail("log.entries", _log.Count.ToString());
        while (_log.Count > MaxEntries)
        {
            _log.TryDequeue(out _);
        }
    }

    private async void OnCommand(string? args)
    {
        if (args is "on")
        {
            _captureEnabled = true;
            UpdateRuntimeFlags();
            SF.Chat.Add("RpcDebugger: capture enabled.");
            return;
        }

        if (args is "off")
        {
            _captureEnabled = false;
            UpdateRuntimeFlags();
            SF.Chat.Add("RpcDebugger: capture disabled.");
            return;
        }

        if (args is "clear")
        {
            ClearCounters();
            UpdateRuntimeFlags();
            SF.Chat.Add("RpcDebugger: log cleared.");
            return;
        }

        await ShowMainMenu();
    }

    private void ClearCounters()
    {
        while (_log.TryDequeue(out _)) { }
        Interlocked.Exchange(ref _totalIncomingRpc, 0);
        Interlocked.Exchange(ref _totalOutgoingRpc, 0);
        Interlocked.Exchange(ref _totalIncomingPacket, 0);
        Interlocked.Exchange(ref _totalOutgoingPacket, 0);
        Context.SetDetail("log.entries", "0");
    }

    private void UpdateRuntimeFlags()
    {
        Context.SetDetail("capture", _captureEnabled ? "on" : "off");
        Context.SetDetail("incoming", _captureIncoming ? "on" : "off");
        Context.SetDetail("outgoing", _captureOutgoing ? "on" : "off");
        Context.SetDetail("rpc", _captureRpc ? "on" : "off");
        Context.SetDetail("packets", _capturePackets ? "on" : "off");
    }

    private async Task ShowMainMenu()
    {
        int inRpc = Volatile.Read(ref _totalIncomingRpc);
        int outRpc = Volatile.Read(ref _totalOutgoingRpc);
        int inPkt = Volatile.Read(ref _totalIncomingPacket);
        int outPkt = Volatile.Read(ref _totalOutgoingPacket);
        int logCount = _log.Count;
        string captureStatus = Toggle(_captureEnabled);
        string inFilter = Toggle(_captureIncoming);
        string outFilter = Toggle(_captureOutgoing);
        string rpcFilter = Toggle(_captureRpc);
        string pktFilter = Toggle(_capturePackets);

        var result = await SF.Dialog.ShowList(
            TitleColor.Apply("Network Debugger"),
            new[]
            {
                $"{Paint(SFColors.Cyan, "View log")}\t{Paint(MutedColor, $"{logCount} entries")}",
                $"{Paint(SFColors.Purple, "View stats")}\t{Paint(MutedColor, $"RPC: {inRpc + outRpc} / PKT: {inPkt + outPkt}")}",
                $"{Paint(SFColors.Yellow, "Toggle capture")}\t{captureStatus}",
                $"{Paint(IncomingColor, "Filter: Incoming")}\t{inFilter}",
                $"{Paint(OutgoingColor, "Filter: Outgoing")}\t{outFilter}",
                $"{Paint(RpcColor, "Filter: RPC")}\t{rpcFilter}",
                $"{Paint(PacketColor, "Filter: Packets")}\t{pktFilter}",
                $"{Paint(DangerColor, "Clear log")}\t{Paint(MutedColor, "Reset all")}"
            },
            $"{HeaderColor.Apply("Action")}\t{HeaderColor.Apply("Info")}"
        );

        if (result.Button != SFDialogButton.OK)
        {
            return;
        }

        switch (result.SelectedIndex)
        {
            case 0:
                await ShowLog(0);
                break;
            case 1:
                await ShowStats();
                break;
            case 2:
                _captureEnabled = !_captureEnabled;
                UpdateRuntimeFlags();
                SF.Chat.Add($"RpcDebugger: capture {(_captureEnabled ? "enabled" : "disabled")}.");
                await ShowMainMenu();
                break;
            case 3:
                _captureIncoming = !_captureIncoming;
                UpdateRuntimeFlags();
                await ShowMainMenu();
                break;
            case 4:
                _captureOutgoing = !_captureOutgoing;
                UpdateRuntimeFlags();
                await ShowMainMenu();
                break;
            case 5:
                _captureRpc = !_captureRpc;
                UpdateRuntimeFlags();
                await ShowMainMenu();
                break;
            case 6:
                _capturePackets = !_capturePackets;
                UpdateRuntimeFlags();
                await ShowMainMenu();
                break;
            case 7:
                ClearCounters();
                SF.Chat.Add("RpcDebugger: log cleared.");
                await ShowMainMenu();
                break;
        }
    }

    private async Task ShowLog(int page)
    {
        NetLogEntry[] entries = _log.ToArray();
        Array.Reverse(entries);

        int totalPages = Math.Max(1, (entries.Length + EntriesPerPage - 1) / EntriesPerPage);
        page = Math.Clamp(page, 0, totalPages - 1);

        int skip = page * EntriesPerPage;
        int pageCount = Math.Min(EntriesPerPage, entries.Length - skip);

        List<string> items = new();
        long now = Environment.TickCount64;

        for (int i = 0; i < pageCount; i++)
        {
            NetLogEntry entry = entries[skip + i];
            string dir = Paint(entry.Direction == Direction.Incoming ? IncomingColor : OutgoingColor, entry.Direction == Direction.Incoming ? "IN" : "OUT");
            string kind = Paint(entry.Kind == MessageKind.Rpc ? RpcColor : PacketColor, entry.Kind == MessageKind.Rpc ? "RPC" : "PKT");
            string name = entry.Name ?? "Unknown";
            long ago = (now - entry.Timestamp) / 1000;
            string time = ago < 60 ? $"{ago}s ago" : $"{ago / 60}m {ago % 60}s ago";
            int dataBytes = (entry.DataBitLength + 7) / 8;

            items.Add($"{dir}\t{kind}\t{Paint(ValueColor, entry.Id.ToString())}\t{Paint(MutedColor, name)}\t{Paint(ValueColor, $"{dataBytes}B")}\t{Paint(SoftMutedColor, time)}");
        }

        if (items.Count == 0)
        {
            items.Add($"{Paint(SoftMutedColor, "-")}\t{Paint(SoftMutedColor, "-")}\t{Paint(SoftMutedColor, "-")}\t{Paint(SoftMutedColor, "No entries")}\t{Paint(SoftMutedColor, "-")}\t{Paint(SoftMutedColor, "-")}");
        }

        int previousPageIndex = -1;
        int nextPageIndex = -1;
        if (totalPages > 1)
        {
            if (page > 0)
            {
                previousPageIndex = items.Count;
                items.Add($"{Paint(NavColor, "<--")}\t{Paint(NavColor, "NAV")}\t{Paint(SoftMutedColor, "-")}\t{Paint(NavColor, "Previous page")}\t{Paint(SoftMutedColor, "-")}\t{Paint(SoftMutedColor, "-")}");
            }

            if (page < totalPages - 1)
            {
                nextPageIndex = items.Count;
                items.Add($"{Paint(NavColor, "-->")}\t{Paint(NavColor, "NAV")}\t{Paint(SoftMutedColor, "-")}\t{Paint(NavColor, "Next page")}\t{Paint(SoftMutedColor, "-")}\t{Paint(SoftMutedColor, "-")}");
            }
        }

        var result = await SF.Dialog.ShowList(
            TitleColor.Apply($"Network Log [Page {page + 1}/{totalPages}]"),
            items,
            $"{HeaderColor.Apply("Dir")}\t{HeaderColor.Apply("Type")}\t{HeaderColor.Apply("ID")}\t{HeaderColor.Apply("Name")}\t{HeaderColor.Apply("Size")}\t{HeaderColor.Apply("Time")}"
        );

        if (result.Button != SFDialogButton.OK)
        {
            if (result.Button == SFDialogButton.Cancel && page > 0)
            {
                await ShowLog(page - 1);
            }

            return;
        }

        if (result.SelectedIndex == previousPageIndex)
        {
            await ShowLog(page - 1);
            return;
        }

        if (result.SelectedIndex == nextPageIndex)
        {
            await ShowLog(page + 1);
            return;
        }

        if (result.SelectedIndex >= 0 && result.SelectedIndex < pageCount)
        {
            await ShowEntryDetail(entries[skip + result.SelectedIndex], page);
        }
    }

    private async Task ShowEntryDetail(NetLogEntry entry, int returnPage)
    {
        string dir = entry.Direction == Direction.Incoming ? "Incoming" : "Outgoing";
        string kind = entry.Kind == MessageKind.Rpc ? "RPC" : "Packet";
        int dataBytes = (entry.DataBitLength + 7) / 8;

        string text = string.Join("\r\n", new[]
        {
            Paint(SFColors.Cyan, $"Direction: {dir}"),
            Paint(SFColors.Cyan, $"Type: {kind}"),
            Paint(ValueColor, $"ID: {entry.Id} ({entry.Name ?? "Unknown"})"),
            Paint(ValueColor, $"Size: {dataBytes} bytes ({entry.DataBitLength} bits)"),
            string.Empty,
            Paint(SFColors.Yellow, "Decoded:"),
            Paint(SFColors.White | SFColors.Ice, entry.Detail ?? "(no decoded data)")
        });

        await SF.Dialog.ShowMessage(TitleColor.Apply($"{kind} {entry.Id} Detail"), text);
        await ShowLog(returnPage);
    }

    private async Task ShowStats()
    {
        NetLogEntry[] entries = _log.ToArray();

        Dictionary<(MessageKind Kind, int Id), (int InCount, int OutCount, int InBits, int OutBits, string? Name)> stats = new();

        foreach (NetLogEntry entry in entries)
        {
            (MessageKind Kind, int Id) key = (entry.Kind, entry.Id);
            if (!stats.TryGetValue(key, out (int InCount, int OutCount, int InBits, int OutBits, string? Name) s))
            {
                s = (0, 0, 0, 0, entry.Name);
            }

            if (entry.Direction == Direction.Incoming)
            {
                s = (s.InCount + 1, s.OutCount, s.InBits + entry.DataBitLength, s.OutBits, s.Name ?? entry.Name);
            }
            else
            {
                s = (s.InCount, s.OutCount + 1, s.InBits, s.OutBits + entry.DataBitLength, s.Name ?? entry.Name);
            }

            stats[key] = s;
        }

        var sorted = stats.OrderByDescending(kv => kv.Value.InCount + kv.Value.OutCount).ToArray();

        List<string> items = new();
        foreach (var (key, s) in sorted)
        {
            string kind = Paint(key.Kind == MessageKind.Rpc ? RpcColor : PacketColor, key.Kind == MessageKind.Rpc ? "RPC" : "PKT");
            string name = s.Name ?? "Unknown";
            int totalBytes = (s.InBits + s.OutBits + 7) / 8;
            items.Add($"{kind}\t{Paint(ValueColor, key.Id.ToString())}\t{Paint(MutedColor, name)}\t{Paint(IncomingColor, s.InCount.ToString())}\t{Paint(OutgoingColor, s.OutCount.ToString())}\t{Paint(ValueColor, $"{totalBytes}B")}");
        }

        if (items.Count == 0)
        {
            items.Add($"{Paint(SoftMutedColor, "-")}\t{Paint(SoftMutedColor, "-")}\t{Paint(SoftMutedColor, "No data")}\t{Paint(SoftMutedColor, "-")}\t{Paint(SoftMutedColor, "-")}\t{Paint(SoftMutedColor, "-")}");
        }

        int inRpc = Volatile.Read(ref _totalIncomingRpc);
        int outRpc = Volatile.Read(ref _totalOutgoingRpc);
        int inPkt = Volatile.Read(ref _totalIncomingPacket);
        int outPkt = Volatile.Read(ref _totalOutgoingPacket);

        await SF.Dialog.ShowList(
            TitleColor.Apply($"Network Stats (RPC: {inRpc + outRpc} / PKT: {inPkt + outPkt})"),
            items,
            $"{HeaderColor.Apply("Type")}\t{HeaderColor.Apply("ID")}\t{HeaderColor.Apply("Name")}\t{HeaderColor.Apply("IN")}\t{HeaderColor.Apply("OUT")}\t{HeaderColor.Apply("Size")}"
        );
    }

    private static string Toggle(bool value)
    {
        return Paint(value ? SuccessColor : DangerColor, value ? "ON" : "OFF");
    }

    private static string Paint(SFColor color, string text)
    {
        return color.Apply(text);
    }
}
