using System.Text;

namespace ConsultNote.Infrastructure;

public static class PhoneNumberFormatter
{
    public static string? Normalize(string? phoneNumber)
    {
        var digits = GetDigits(phoneNumber);
        return string.IsNullOrWhiteSpace(digits) ? null : digits;
    }

    public static string Format(string? phoneNumber)
    {
        var digits = GetDigits(phoneNumber);
        if (string.IsNullOrWhiteSpace(digits))
        {
            return string.Empty;
        }

        if (digits.Length == 11)
        {
            return $"{digits[..3]}-{digits[3..7]}-{digits[7..]}";
        }

        if (digits.Length == 10)
        {
            return digits.StartsWith("02", StringComparison.Ordinal)
                ? $"{digits[..2]}-{digits[2..6]}-{digits[6..]}"
                : $"{digits[..3]}-{digits[3..6]}-{digits[6..]}";
        }

        if (digits.Length == 9 && digits.StartsWith("02", StringComparison.Ordinal))
        {
            return $"{digits[..2]}-{digits[2..5]}-{digits[5..]}";
        }

        return phoneNumber?.Trim() ?? string.Empty;
    }

    public static bool Contains(string? phoneNumber, string keyword)
    {
        var formattedPhoneNumber = Format(phoneNumber);
        var normalizedPhoneNumber = Normalize(phoneNumber);

        return formattedPhoneNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            || (IsPhoneSearchKeyword(keyword)
                && !string.IsNullOrWhiteSpace(normalizedPhoneNumber)
                && Normalize(keyword) is { Length: > 0 } normalizedKeyword
                && !string.IsNullOrWhiteSpace(normalizedKeyword)
                && normalizedPhoneNumber.Contains(normalizedKeyword, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsPhoneSearchKeyword(string keyword)
    {
        return keyword.Any(char.IsDigit)
            && keyword.All(character =>
                char.IsDigit(character) ||
                char.IsWhiteSpace(character) ||
                character is '-' or '.' or '(' or ')' or '+');
    }

    private static string GetDigits(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            if (char.IsDigit(character))
            {
                builder.Append(character);
            }
        }

        return builder.ToString();
    }
}
