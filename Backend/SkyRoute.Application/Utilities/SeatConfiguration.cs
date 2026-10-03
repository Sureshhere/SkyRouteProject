namespace SkyRoute.Application.Utilities;

public static class SeatConfiguration
{
    public static IReadOnlyList<string> GetSeatsForCabin(string cabinClass)
    {
        return cabinClass switch
        {
            "Economy"    => GenerateSeats(30, "ABCDEF"),
            "Business"   => GenerateSeats(10, "ABCD"),
            "FirstClass" => GenerateSeats(5,  "AB"),
            _            => throw new ArgumentException($"Unknown cabin class: '{cabinClass}'.", nameof(cabinClass))
        };
    }

    private static IReadOnlyList<string> GenerateSeats(int rows, string columns)
    {
        var seats = new List<string>(rows * columns.Length);
        for (var row = 1; row <= rows; row++)
            foreach (var col in columns)
                seats.Add($"{row}{col}");
        return seats.AsReadOnly();
    }
}
