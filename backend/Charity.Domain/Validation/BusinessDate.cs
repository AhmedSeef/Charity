using System.Globalization;
using Charity.Domain.Exceptions;

namespace Charity.Domain.Validation;

public static class BusinessDate
{
    public static DateOnly TodayInCairo() => DateOnly.FromDateTime(
        TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Egypt Standard Time"));

    public static string ValidateTodayOrEarlier(string? value, string field)
    {
        var today = TodayInCairo();
        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationDomainException($"{field} مطلوب بصيغة yyyy-MM-dd.");
        if (!DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var date))
            throw new ValidationDomainException($"{field} يجب أن يكون تاريخًا صحيحًا بصيغة yyyy-MM-dd.");
        if (date > today)
            throw new ValidationDomainException($"{field} لا يمكن أن يكون في المستقبل.");
        return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }
}
