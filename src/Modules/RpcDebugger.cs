using System.Collections.Concurrent;

using SFSharp;

public class RpcDebugger : ISFModule
{
    private const int MaxEntries = 200;
    private const int EntriesPerPage = 20;

    private readonly ConcurrentQueue<NetLogEntry> _log = new();
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
        int DataBitLength,
        long Timestamp);

    public async Task RunAsync(CancellationToken token)
    {
        using var command = SF.Chat.RegisterChatCommand("rpcd", OnCommand);

        List<RpcSubscription> subscriptions = new();
        try
        {
            // Subscribe to all known RPC IDs
            foreach (RpcId rpcId in Enum.GetValues<RpcId>())
            {
                subscriptions.Add(SF.Rpc.Subscribe(rpcId, args => OnIncomingRpc(args)));
                subscriptions.Add(SF.Rpc.SubscribeOutgoing(rpcId, args => OnOutgoingRpc(args)));
            }

            // Subscribe to all known Packet IDs
            foreach (PacketId packetId in Enum.GetValues<PacketId>())
            {
                subscriptions.Add(SF.Packets.SubscribeIncoming(packetId, args => OnIncomingPacket(args)));
                subscriptions.Add(SF.Packets.SubscribeOutgoing(packetId, args => OnOutgoingPacket(args)));
            }

            while (!token.IsCancellationRequested)
            {
                await Task.Yield();
            }
        }
        finally
        {
            foreach (var sub in subscriptions)
            {
                sub.Dispose();
            }
        }
    }

    private void OnIncomingRpc(IncomingRpcArgs args)
    {
        Interlocked.Increment(ref _totalIncomingRpc);
        if (!_captureEnabled || !_captureIncoming || !_captureRpc) return;

        string? name = Enum.IsDefined((RpcId)args.RpcId) ? ((RpcId)args.RpcId).ToString() : null;
        Enqueue(new NetLogEntry(Direction.Incoming, MessageKind.Rpc, args.RpcId, name, args.DataBitLength, Environment.TickCount64));
    }

    private void OnOutgoingRpc(OutgoingRpcArgs args)
    {
        Interlocked.Increment(ref _totalOutgoingRpc);
        if (!_captureEnabled || !_captureOutgoing || !_captureRpc) return;

        string? name = Enum.IsDefined((RpcId)args.RpcId) ? ((RpcId)args.RpcId).ToString() : null;
        Enqueue(new NetLogEntry(Direction.Outgoing, MessageKind.Rpc, args.RpcId, name, args.DataBitLength, Environment.TickCount64));
    }

    private void OnIncomingPacket(IncomingPacketArgs args)
    {
        Interlocked.Increment(ref _totalIncomingPacket);
        if (!_captureEnabled || !_captureIncoming || !_capturePackets) return;

        string? name = Enum.IsDefined((PacketId)args.PacketId) ? ((PacketId)args.PacketId).ToString() : null;
        Enqueue(new NetLogEntry(Direction.Incoming, MessageKind.Packet, args.PacketId, name, args.DataBitLength, Environment.TickCount64));
    }

    private void OnOutgoingPacket(OutgoingPacketArgs args)
    {
        Interlocked.Increment(ref _totalOutgoingPacket);
        if (!_captureEnabled || !_captureOutgoing || !_capturePackets) return;

        string? name = Enum.IsDefined((PacketId)args.PacketId) ? ((PacketId)args.PacketId).ToString() : null;
        Enqueue(new NetLogEntry(Direction.Outgoing, MessageKind.Packet, args.PacketId, name, args.DataBitLength, Environment.TickCount64));
    }

    private void Enqueue(NetLogEntry entry)
    {
        _log.Enqueue(entry);
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
            SF.Chat.Add("RpcDebugger: capture enabled.");
            return;
        }

        if (args is "off")
        {
            _captureEnabled = false;
            SF.Chat.Add("RpcDebugger: capture disabled.");
            return;
        }

        if (args is "clear")
        {
            ClearCounters();
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
    }

    private async Task ShowMainMenu()
    {
        int inRpc = Volatile.Read(ref _totalIncomingRpc);
        int outRpc = Volatile.Read(ref _totalOutgoingRpc);
        int inPkt = Volatile.Read(ref _totalIncomingPacket);
        int outPkt = Volatile.Read(ref _totalOutgoingPacket);
        int logCount = _log.Count;
        string captureStatus = _captureEnabled ? "{00FF00}ON" : "{FF0000}OFF";
        string inFilter = _captureIncoming ? "{00FF00}ON" : "{FF0000}OFF";
        string outFilter = _captureOutgoing ? "{00FF00}ON" : "{FF0000}OFF";
        string rpcFilter = _captureRpc ? "{00FF00}ON" : "{FF0000}OFF";
        string pktFilter = _capturePackets ? "{00FF00}ON" : "{FF0000}OFF";

        var result = await SF.Dialog.ShowList(
            "Network Debugger",
            new[]
            {
                $"View log\t{{AAAAAA}}{logCount} entries",
                $"View stats\t{{AAAAAA}}RPC: {inRpc + outRpc} / PKT: {inPkt + outPkt}",
                $"Toggle capture\t{captureStatus}",
                $"Filter: Incoming\t{inFilter}",
                $"Filter: Outgoing\t{outFilter}",
                $"Filter: RPC\t{rpcFilter}",
                $"Filter: Packets\t{pktFilter}",
                $"Clear log\t{{AAAAAA}}Reset all"
            },
            "Action\tInfo"
        );

        if (result.Button != SFDialogButton.OK) return;

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
                SF.Chat.Add($"RpcDebugger: capture {(_captureEnabled ? "enabled" : "disabled")}.");
                await ShowMainMenu();
                break;
            case 3:
                _captureIncoming = !_captureIncoming;
                await ShowMainMenu();
                break;
            case 4:
                _captureOutgoing = !_captureOutgoing;
                await ShowMainMenu();
                break;
            case 5:
                _captureRpc = !_captureRpc;
                await ShowMainMenu();
                break;
            case 6:
                _capturePackets = !_capturePackets;
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
        var pageEntries = entries.AsSpan().Slice(skip, Math.Min(EntriesPerPage, entries.Length - skip));

        List<string> items = new();
        long now = Environment.TickCount64;

        foreach (ref readonly NetLogEntry entry in pageEntries)
        {
            string dir = entry.Direction == Direction.Incoming ? "{00AAFF}IN" : "{FFAA00}OUT";
            string kind = entry.Kind == MessageKind.Rpc ? "{CCCCFF}RPC" : "{FFCCAA}PKT";
            string name = entry.Name ?? "Unknown";
            long ago = (now - entry.Timestamp) / 1000;
            string time = ago < 60 ? $"{ago}s ago" : $"{ago / 60}m {ago % 60}s ago";
            int dataBytes = (entry.DataBitLength + 7) / 8;

            items.Add($"{dir}\t{kind}\t{{FFFFFF}}{entry.Id}\t{{AAAAAA}}{name}\t{{FFFFFF}}{dataBytes}B\t{{888888}}{time}");
        }

        if (items.Count == 0)
        {
            items.Add("{888888}-\t-\t-\tNo entries\t-\t-");
        }

        string navHint = $"Page {page + 1}/{totalPages}";
        if (page < totalPages - 1) navHint += " | OK=Next";
        if (page > 0) navHint += " | Cancel=Back";

        var result = await SF.Dialog.ShowList(
            $"Network Log [{navHint}]",
            items,
            "Dir\tType\tID\tName\tSize\tTime"
        );

        if (result.Button == SFDialogButton.OK && page < totalPages - 1)
        {
            await ShowLog(page + 1);
        }
        else if (result.Button == SFDialogButton.Cancel && page > 0)
        {
            await ShowLog(page - 1);
        }
    }

    private async Task ShowStats()
    {
        NetLogEntry[] entries = _log.ToArray();

        Dictionary<(MessageKind Kind, int Id), (int InCount, int OutCount, int InBits, int OutBits, string? Name)> stats = new();

        foreach (ref readonly NetLogEntry entry in entries.AsSpan())
        {
            var key = (entry.Kind, entry.Id);
            if (!stats.TryGetValue(key, out var s))
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
            string kind = key.Kind == MessageKind.Rpc ? "{CCCCFF}RPC" : "{FFCCAA}PKT";
            string name = s.Name ?? "Unknown";
            int totalBytes = (s.InBits + s.OutBits + 7) / 8;
            items.Add($"{kind}\t{{FFFFFF}}{key.Id}\t{{AAAAAA}}{name}\t{{00AAFF}}{s.InCount}\t{{FFAA00}}{s.OutCount}\t{{FFFFFF}}{totalBytes}B");
        }

        if (items.Count == 0)
        {
            items.Add("-\t-\tNo data\t-\t-\t-");
        }

        int inRpc = Volatile.Read(ref _totalIncomingRpc);
        int outRpc = Volatile.Read(ref _totalOutgoingRpc);
        int inPkt = Volatile.Read(ref _totalIncomingPacket);
        int outPkt = Volatile.Read(ref _totalOutgoingPacket);

        await SF.Dialog.ShowList(
            $"Network Stats (RPC: {inRpc + outRpc} / PKT: {inPkt + outPkt})",
            items,
            "Type\tID\tName\tIN\tOUT\tSize"
        );
    }
}
