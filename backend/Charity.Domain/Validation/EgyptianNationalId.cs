using System.Globalization;
using Charity.Domain.Exceptions;

namespace Charity.Domain.Validation;

/// <summary>
/// Structural validation only; no unpublished check-digit algorithm or identity-record verification.
/// Reference: https://vlenseg.com/en/egypt-national-id-api/
/// </summary>
public static class EgyptianNationalId
{
    private static readonly HashSet<string> Governorates =
    ["01", "02", "03", "04", "11", "12", "13", "14", "15", "16", "17", "18", "19",
     "21", "22", "23", "24", "25", "26", "27", "28", "29", "31", "32", "33", "34", "35", "88"];

    public static string Validate(string? value, string field = "الرقم القومي", DateOnly? today = null)
    {
        var normalized = new string((value ?? "").Trim().Select(c => c switch
        {
            >= '٠' and <= '٩' => (char)('0' + c - '٠'),
            >= '۰' and <= '۹' => (char)('0' + c - '۰'),
            _ => c
        }).ToArray());

        if (normalized.Length != 14 || normalized.Any(c => c < '0' || c > '9'))
            throw new ValidationDomainException($"{field}: يجب أن يتكون من 14 رقماً فقط.");
        if (normalized[0] is not ('2' or '3'))
            throw new ValidationDomainException($"{field}: كود القرن يجب أن يكون 2 لمواليد 1900–1999 أو 3 لمواليد 2000–2099.");

        var dateText = (normalized[0] == '2' ? "19" : "20") + normalized.Substring(1, 6);
        if (!DateOnly.TryParseExact(dateText, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var birthDate))
            throw new ValidationDomainException($"{field}: تاريخ الميلاد داخل الرقم غير صحيح.");
        var cairoToday = today ?? DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Egypt Standard Time"));
        if (birthDate > cairoToday)
            throw new ValidationDomainException($"{field}: تاريخ الميلاد لا يمكن أن يكون في المستقبل.");
        if (!Governorates.Contains(normalized.Substring(7, 2)))
            throw new ValidationDomainException($"{field}: كود محافظة الميلاد غير صحيح (88 لمواليد خارج مصر).");
        return normalized;
    }
}
