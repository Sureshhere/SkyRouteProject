using System.Text.RegularExpressions;

namespace SkyRoute.Application.Validators;

public static class DocumentNumberValidator
{
    // National ID: 8–12 alphanumeric characters
    private static readonly Regex NationalIdRegex = new(@"^[A-Za-z0-9]{8,12}$", RegexOptions.Compiled);

    // Passport: 6–9 alphanumeric characters
    private static readonly Regex PassportRegex = new(@"^[A-Za-z0-9]{6,9}$", RegexOptions.Compiled);

    public static bool IsValidNationalId(string documentNumber)
    {
        if (string.IsNullOrWhiteSpace(documentNumber)) return false;
        return NationalIdRegex.IsMatch(documentNumber);
    }

    public static bool IsValidPassport(string documentNumber)
    {
        if (string.IsNullOrWhiteSpace(documentNumber)) return false;
        return PassportRegex.IsMatch(documentNumber);
    }

    public static bool IsDomesticRoute(string originCountryCode, string destinationCountryCode)
    {
        return string.Equals(originCountryCode, destinationCountryCode, StringComparison.OrdinalIgnoreCase);
    }
}
