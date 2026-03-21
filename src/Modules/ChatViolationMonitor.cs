using SFSharp;
using System.Text.RegularExpressions;

public record ChatViolationRecord(DateTime Timestamp, string PlayerName, string ViolationType, string MatchedFragment, string OriginalText);

[SFModule("chat-violation-monitor", "ChatViolationMonitor", Category = "Moderation", Description = "Tracks rule-breaking chat fragments and stores recent violations.", ExecutionModel = ModuleExecutionModel.MainThread, Order = 50)]
public class ChatViolationMonitor : SFModuleBase
{
    private const int MaxRecords = 200;
    private readonly object _sync = new();
    private readonly List<ChatViolationRecord> _records = new();

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using IDisposable violationsCommand = Context.RegisterChatCommand("violations", OnCommand);
        using IDisposable shortCommand = Context.RegisterChatCommand("viollog", OnCommand);

        await foreach (ServerChatEntry entry in SF.Chat.StreamServerChatEntries(cancellationToken))
        {
            Context.IncrementCounter("chat.entries");

            if (entry.Kind != ServerChatKind.ClientMessage)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(entry.Text))
            {
                continue;
            }

            string normalizedText = NormalizeForDetection(entry.Text);
            if (!TryParsePlayerMessage(normalizedText, out string playerName, out string messageText))
            {
                continue;
            }

            if (!TryDetectViolation(messageText, out string violationType, out string matchedFragment))
            {
                continue;
            }

            AddRecord(new(DateTime.Now, playerName, violationType, matchedFragment, entry.Text));
            Context.IncrementCounter("violations.detected");
            Context.SetDetail("records", GetSnapshot().Length.ToString());
            Context.Heartbeat($"violation:{violationType}");
            Log.Info($"Violation detected player={playerName} type={violationType} fragment={matchedFragment}");
        }
    }

    private bool TryParsePlayerMessage(string text, out string playerName, out string messageText)
    {
        Match match = ChatModerationRegexHelper.PlayerChatLine().Match(text);
        if (!match.Success)
        {
            playerName = string.Empty;
            messageText = string.Empty;
            return false;
        }

        playerName = match.Groups["name"].Value;
        messageText = match.Groups["message"].Value.Trim();
        return true;
    }

    private bool TryDetectViolation(string messageText, out string violationType, out string matchedFragment)
    {
        Match familyInsultMatch = ChatModerationRegexHelper.FamilyInsultText().Match(messageText);
        if (familyInsultMatch.Success)
        {
            violationType = "Оскорбление родных";
            matchedFragment = familyInsultMatch.Value.Trim();
            return true;
        }

        Match telegramAdMatch = ChatModerationRegexHelper.TelegramAdText().Match(messageText);
        if (telegramAdMatch.Success)
        {
            violationType = "Реклама Telegram";
            matchedFragment = telegramAdMatch.Value.Trim();
            return true;
        }

        violationType = string.Empty;
        matchedFragment = string.Empty;
        return false;
    }

    private void AddRecord(ChatViolationRecord record)
    {
        lock (_sync)
        {
            _records.Insert(0, record);
            if (_records.Count > MaxRecords)
            {
                _records.RemoveAt(_records.Count - 1);
            }
        }
    }

    private ChatViolationRecord[] GetSnapshot()
    {
        lock (_sync)
        {
            return _records.ToArray();
        }
    }

    private async void OnCommand(string? args)
    {
        while (true)
        {
            ChatViolationRecord[] snapshot = GetSnapshot();
            if (snapshot.Length == 0)
            {
                SF.Chat.Add("Лог нарушений пуст.");
                return;
            }

            string header = "Время\tИгрок\tТип\tФрагмент";
            string[] items = snapshot
                .Select(x => $"{x.Timestamp:HH:mm:ss}\t{x.PlayerName}\t{x.ViolationType}\t{TrimForList(x.MatchedFragment)}")
                .ToArray();

            var result = await SF.Dialog.ShowList("Лог нарушений чата", items, header);
            if (result.Button != SFDialogButton.OK)
            {
                return;
            }

            ChatViolationRecord record = snapshot[result.SelectedIndex];
            string detailText = string.Join("\r\n", new string[]
            {
                $"Игрок: {record.PlayerName}",
                $"Тип нарушения: {record.ViolationType}",
                $"Совпадение: {record.MatchedFragment}",
                $"Время: {record.Timestamp:dd.MM.yyyy HH:mm:ss}",
                string.Empty,
                "Полный текст:",
                record.OriginalText,
                string.Empty,
                "Нажмите OK, чтобы скопировать ник нарушителя."
            });

            SFDialogButton detailResult = await SF.Dialog.ShowMessage("Информация о нарушении", detailText);
            if (detailResult == SFDialogButton.None)
            {
                return;
            }

            if (detailResult == SFDialogButton.OK)
            {
                if (Win32.TrySetClipboardText(record.PlayerName))
                {
                    SF.Chat.Add($"Ник {record.PlayerName} скопирован в буфер обмена.");
                }
                else
                {
                    SF.Chat.Add("Не удалось скопировать ник в буфер обмена.");
                }
            }
        }
    }

    private static string NormalizeForDetection(string value)
    {
        string withoutColors = ChatModerationRegexHelper.ColorCode().Replace(value, " ");
        return ChatModerationRegexHelper.Whitespace().Replace(withoutColors, " ").Trim();
    }

    private static string TrimForList(string value)
    {
        const int maxLength = 36;
        string sanitized = NormalizeForDetection(value);
        return sanitized.Length <= maxLength ? sanitized : sanitized[..(maxLength - 3)] + "...";
    }
}

public static partial class ChatModerationRegexHelper
{
    [GeneratedRegex(
        """^\s*(?:\[[^\]]+\]\s*)*(?:\{[0-9A-Fa-f]{6,8}\})*(?<name>[A-Za-z0-9_]+)\[(?<id>\d+)\](?::|\s+[^:\r\n]+:)\s*(?<message>.+)$""",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    public static partial Regex PlayerChatLine();

    [GeneratedRegex("""\{[0-9A-Fa-f]{6,8}\}""")]
    public static partial Regex ColorCode();

    [GeneratedRegex("""\s+""")]
    public static partial Regex Whitespace();

    [GeneratedRegex(
        """
        (?ix)
        (?:(?<abuse>еб(?:ал|ать|у|ешь|ан(?:у|ый|ая|ое|ые)?|н(?:ый|ая|ое|ые)?)|ёб(?:ан(?:ый|ая|ое|ые)?|ал|у|н)|уеб(?:ок|ан)?|заеб(?:ал|у|ан)|пид(?:ор|ар|р)|ху(?:й|я|е|и)|xуй|хер|муд(?:ак|ила)|шлюх(?:а|у|ой)|сос(?:и|ал|ет)|гандон|долбо(?:еб|ёб)|трах(?:ал|ну)|сдох|умри)(?:[^\p{L}\n]+|\s+\w+\s+){0,3}(?<target>мам(?:а|у|е|ой|к(?:а|у|е))|мать|бат(?:я|ю|е|ьк\w*)|от(?:е|ё)?ц(?:а|у|е|ом)?|родн(?:я|ю|ых|ым|ыми)|семь(?:я|ю|и)|сест(?:ра|ру|ре|рой|ры)|брат(?:а|у|е|ом)?))
        |
        (?:(?<target>мам(?:а|у|е|ой|к(?:а|у|е))|мать|бат(?:я|ю|е|ьк\w*)|от(?:е|ё)?ц(?:а|у|е|ом)?|родн(?:я|ю|ых|ым|ыми)|семь(?:я|ю|и)|сест(?:ра|ру|ре|рой|ры)|брат(?:а|у|е|ом)?)(?:[^\p{L}\n]+|\s+\w+\s+){0,3}(?<abuse>еб(?:ал|ать|у|ешь|ан(?:у|ый|ая|ое|ые)?|н(?:ый|ая|ое|ые)?)|ёб(?:ан(?:ый|ая|ое|ые)?|ал|у|н)|уеб(?:ок|ан)?|заеб(?:ал|у|ан)|пид(?:ор|ар|р)|ху(?:й|я|е|и)|xуй|хер|муд(?:ак|ила)|шлюх(?:а|у|ой)|сос(?:и|ал|ет)|гандон|долбо(?:еб|ёб)|трах(?:ал|ну)|сдох|умри))
        """,
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    public static partial Regex FamilyInsultText();

    [GeneratedRegex(
        """
        (?ix)
        (?:(?:t\.me/|telegram(?:\.me)?/|телеграм|telegram|\btg\b|\bтг\b|@[A-Za-z0-9_]{4,}).{0,40}(?:канал|бот|подпис|розыгрыш|казин|ставк|промокод|слив|схем|заработ|ссылка|join|bonus|hack|soft|чит|обход|вип))
        |
        (?:(?:канал|бот|подпис|розыгрыш|казин|ставк|промокод|слив|схем|заработ|ссылка|join|bonus|hack|soft|чит|обход|вип).{0,40}(?:t\.me/|telegram(?:\.me)?/|телеграм|telegram|\btg\b|\bтг\b|@[A-Za-z0-9_]{4,}))
        """,
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    public static partial Regex TelegramAdText();
}



