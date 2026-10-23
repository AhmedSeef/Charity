using Charity.Domain.Exceptions;
using Charity.Domain.Validation;

namespace Charity.UnitTests;

public class EgyptianNationalIdTests
{
    private static readonly DateOnly Today = new(2026, 10, 23);

    [Theory]
    [InlineData("28501011234567")]
    [InlineData("30002290112345")]
    [InlineData("22610230112345")]
    [InlineData("32610230112345")]
    [InlineData("29912318812345")]
    [InlineData("30001010112340")]
    public void Valid_structure_is_accepted_without_unpublished_checksum_or_age_rules(string id)
        => Assert.Equal(id, EgyptianNationalId.Validate(id, today: Today));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("2850101123456")]
    [InlineData("285010112345678")]
    [InlineData("2850101123456x")]
    [InlineData("18501011234567")]
    [InlineData("48501011234567")]
    [InlineData("28500011234567")]
    [InlineData("28513011234567")]
    [InlineData("28501001234567")]
    [InlineData("28504311234567")]
    [InlineData("29902291234567")]
    [InlineData("30002301234567")]
    [InlineData("32610240112345")]
    [InlineData("39901010112345")]
    [InlineData("28501010012345")]
    [InlineData("28501010512345")]
    [InlineData("28501012012345")]
    [InlineData("28501013012345")]
    [InlineData("28501019912345")]
    [InlineData("2850101 123456")]
    [InlineData("２８５０１０１１２３４５６７")]
    public void Invalid_structure_is_rejected(string? id)
        => Assert.Throws<ValidationDomainException>(() => EgyptianNationalId.Validate(id, today: Today));

    [Theory]
    [InlineData(" ٢٨٥٠١٠١١٢٣٤٥٦٧ ")]
    [InlineData("۲۸۵۰۱۰۱۱۲۳۴۵۶۷")]
    [InlineData("٢85٠10١12٣45٦7")]
    public void Arabic_and_persian_digits_are_normalized_before_validation(string id)
        => Assert.Equal("28501011234567", EgyptianNationalId.Validate(id, today: Today));

    [Theory]
    [InlineData("01")][InlineData("02")][InlineData("03")][InlineData("04")]
    [InlineData("11")][InlineData("12")][InlineData("13")][InlineData("14")][InlineData("15")]
    [InlineData("16")][InlineData("17")][InlineData("18")][InlineData("19")]
    [InlineData("21")][InlineData("22")][InlineData("23")][InlineData("24")][InlineData("25")]
    [InlineData("26")][InlineData("27")][InlineData("28")][InlineData("29")]
    [InlineData("31")][InlineData("32")][InlineData("33")][InlineData("34")][InlineData("35")][InlineData("88")]
    public void All_assigned_birth_governorate_codes_are_accepted(string code)
        => Assert.Equal($"2850101{code}12345", EgyptianNationalId.Validate($"2850101{code}12345", today: Today));
}
