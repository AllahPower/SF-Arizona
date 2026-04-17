using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace SFSharp.Runtime.Modules;

public enum LicenseType
{
    Driving,
    Fish,
    Boat,
    Heli,
    Gun,
    Plane,
    Business,
    Hunting
}


[JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = true)]
[JsonSerializable(typeof(LicenseSale))]
[JsonSerializable(typeof(List<LicenseSale>))]
internal partial class SourceGenerationContext : JsonSerializerContext;
public record LicenseSale(string Name, LicenseType LicenseType, int Price, DateTime DateTime);

[SFModule("license-shooter", "LicenseShooter", Category = "Tracking", Description = "Tracks license offers and persists successful sales.", ExecutionModel = ModuleExecutionModel.MainThread, Order = 40)]
public class LicenseShooter : SFModuleBase
{
    private const string SalesFileName = "licenses.json";

    private List<LicenseSale> LoadSales()
    {
        if (!Context.UserData.Exists(SalesFileName)) return [];
        var text = Context.UserData.ReadAllText(SalesFileName);
        if (string.IsNullOrWhiteSpace(text)) return [];
        return JsonSerializer.Deserialize(text, SourceGenerationContext.Default.ListLicenseSale)!;
    }

    private void SaveSales(List<LicenseSale> sales)
    {
        var text = JsonSerializer.Serialize(sales, SourceGenerationContext.Default.ListLicenseSale);
        Context.UserData.WriteAllText(SalesFileName, text);
    }

    private LicenseType? GetLicenseType(string licenseName)
    {
        return licenseName switch
        {
            "лицензию на вождение" => LicenseType.Driving,
            "лицензию на рыболовство" => LicenseType.Fish,
            "лицензию на морской транспорт" => LicenseType.Boat,
            "лицензию на вертолеты" => LicenseType.Heli,
            "лицензию на оружие" => LicenseType.Gun,
            "лицензию на самолеты" => LicenseType.Plane,
            "разрешение на бизнес" => LicenseType.Business,
            "лицензию на охоту" => LicenseType.Hunting,
            _ => null
        };
    }

    private string GetLicenseDisplayText(LicenseType type)
    {
        return type switch
        {
            LicenseType.Driving => "Лицензия на вождение",
            LicenseType.Fish => "Лицензия на рыболовство",
            LicenseType.Boat => "Лицензия на морской транспорт",
            LicenseType.Heli => "Лицензия на вертолеты",
            LicenseType.Gun => "Лицензия на оружие",
            LicenseType.Plane => "Лицензия на самолеты",
            LicenseType.Business => "Разрешение на бизнес",
            LicenseType.Hunting => "Лицензия на охоту",
            _ => throw new UnreachableException()
        };
    }

    private bool TryDecodeOffer(string text,[NotNullWhen(true)] out LicenseSale? sale)
    {
        var match = RegexHelper.LicenseOfferText().Match(text);
        if (!match.Success)
        {
            sale = default;
            return false;
        }
        var name = match.Groups["name"].Value;
        var licenseName = match.Groups["licenseName"].Value;
        var priceStr = match.Groups["price"].Value;
        if (!int.TryParse(priceStr, out var price))
        {
            sale = default;
            return false;
        }
        var licenseType = GetLicenseType(licenseName);
        if (licenseType is not LicenseType type)
        {
            sale = default;
            return false;
        }
        sale = new(name, type, price, DateTime.Now);
        return true;
    }

    private bool TryDecodeSale(string text, [NotNullWhen(true)] out string? name)
    {
        var match = RegexHelper.LicenseSaleText().Match(text);
        if (!match.Success)
        {
            name = default;
            return false;
        }
        name = match.Groups["name"].Value;
        return true;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        ISF sf = Context.SF;
        using IDisposable commandRegistration = Context.RegisterChatCommand("licenselog", OnCommand);

        var offers = new Dictionary<string, LicenseSale>();

        await foreach(var entry in SF.Chat.StreamChatEntries(cancellationToken))
        {
            Context.IncrementCounter("chat.entries");
            if (entry.TextColor != 0xFF6495ED) continue;
            if (entry.Text is null) continue;

            if (TryDecodeOffer(entry.Text, out var offer))
            {
                offers[offer.Name] = offer;
                Context.IncrementCounter("offers.detected");
                Context.SetDetail("pending.offers", offers.Count.ToString());
                Context.Heartbeat("offer-detected");
                continue;
            }
            if (TryDecodeSale(entry.Text, out var name))
            {
                if(!offers.TryGetValue(name, out var previousOffer))
                {
                    sf.Chat.Add($"Detected sale {name}, but could not find an offer.");
                    Context.IncrementCounter("sales.missed");
                    continue;
                }

                var sales = LoadSales();
                sales.Insert(0, previousOffer);
                SaveSales(sales);

                offers.Remove(name);
                Context.IncrementCounter("sales.recorded");
                Context.SetDetail("pending.offers", offers.Count.ToString());
                Context.SetDetail("last.sale", name);
                Context.Heartbeat("sale-recorded");
                sf.Chat.Add($"Recorded sale to {name}");
                continue;
            }
        }
    }

    private async void OnCommand(string? args)
    {
        ISF sf = Context.SF;
        while (true)
        {
            var dateListResult = await ShowDateList();
            if (dateListResult is not DateOnly date) return;

            while (true)
            {
                var saleListResult = await ShowSaleList(date);
                if (saleListResult.Button == SFDialogButton.None) return;
                if (saleListResult.Button == SFDialogButton.Cancel) break;

                var confirmationResult = await sf.Dialog.ShowMessage("Подтверждение", "Удалить выбранную запись?");
                if (confirmationResult == SFDialogButton.None) return;
                if (confirmationResult == SFDialogButton.Cancel) continue;

                var sale = saleListResult.Sale!;
                var list = LoadSales();
                var index = list.FindIndex(x => x == sale); // Records override the == operator
                list.RemoveAt(index);
                SaveSales(list);
            }
        }
        
    }

    private async Task<DateOnly?> ShowDateList()
    {
        ISFDialog dialog = Context.SF.Dialog;
        var groupedSales = LoadSales()
            .GroupBy(x => DateOnly.FromDateTime(x.DateTime))
            .OrderByDescending(x => x.Key)
            .Select(x => (Date: x.Key, Sales: x.ToArray()))
            .ToArray();

        var header = "Дата\tКоличество\tСумма";
        var items = groupedSales
            .Select(x => $"{x.Date.ToShortDateString()}\t{x.Sales.Length}\t{x.Sales.Sum(x => x.Price)}")
            .ToArray();
        var result = await dialog.ShowList("Проданные лицензии", items, header);
        if (result.Button != SFDialogButton.OK) return null;
        return groupedSales[result.SelectedIndex].Date;
    }

    private async Task<(SFDialogButton Button, LicenseSale? Sale)> ShowSaleList(DateOnly date)
    {
        ISFDialog dialog = Context.SF.Dialog;
        var sales = LoadSales()
            .Where(x => DateOnly.FromDateTime(x.DateTime) == date)
            .OrderByDescending(x => x.DateTime)
            .ToArray();

        var header = "Лицензия\tПокупатель\tВремя\tСумма";
        var items = sales
            .Select(x => $"{GetLicenseDisplayText(x.LicenseType)}\t{x.Name}\t{x.DateTime.ToShortTimeString()}\t{x.Price}")
            .ToArray();
        var result = await dialog.ShowList($"Проданные лицензии за {date.ToShortDateString()}", items, header);
        if(result.Button != SFDialogButton.OK)
        {
            return (result.Button, null);
        }
        return (SFDialogButton.OK, sales[result.SelectedIndex]);
    }
}

// Вы предложили `Joseph_Bright` купить `лицензию на охоту`, за `5000` вирт
public static partial class RegexHelper
{
    [GeneratedRegex(@"\A Вы предложили (?<name>.+) купить (?<licenseName>.+), за (?<price>.+) вирт\z")]
    public static partial Regex LicenseOfferText();

    [GeneratedRegex(@"\A (?<name>.+) купил лицензию\z")]
    public static partial Regex LicenseSaleText();
}
