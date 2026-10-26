using OfficeOpenXml;

namespace QuickCart.Tests.Support;

public sealed record CheckoutCase(string Id, string Name, string Email, string Postcode, string ExpectedError);

public sealed record ProductRow(int Id, string Name, string Category, double Price, int Stock);

/// <summary>Reads parameterised test data from TestData/quickcart-test-data.xlsx with EPPlus.</summary>
public static class ExcelTestData
{
    private static readonly string WorkbookPath =
        Path.Combine(AppContext.BaseDirectory, "TestData", "quickcart-test-data.xlsx");

    static ExcelTestData()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public static CheckoutCase GetCheckoutCase(string id)
    {
        var match = ReadSheet("Checkout", row => new CheckoutCase(
                row[0], row[1], row[2], row[3], row[4]))
            .FirstOrDefault(c => c.Id == id);
        return match ?? throw new ArgumentException($"No checkout test data row with id '{id}' in {WorkbookPath}");
    }

    public static IReadOnlyList<ProductRow> GetProducts() =>
        ReadSheet("Products", row => new ProductRow(
            int.Parse(row[0]),
            row[1],
            row[2],
            double.Parse(row[3], System.Globalization.CultureInfo.InvariantCulture),
            int.Parse(row[4])));

    private static List<T> ReadSheet<T>(string sheetName, Func<string[], T> map)
    {
        using var package = new ExcelPackage(new FileInfo(WorkbookPath));
        var sheet = package.Workbook.Worksheets[sheetName]
                    ?? throw new InvalidOperationException($"Sheet '{sheetName}' not found in {WorkbookPath}");

        var result = new List<T>();
        var lastRow = sheet.Dimension.End.Row;
        var lastColumn = sheet.Dimension.End.Column;

        // Row 1 holds the column headers.
        for (var r = 2; r <= lastRow; r++)
        {
            var values = new string[lastColumn];
            for (var c = 1; c <= lastColumn; c++)
            {
                values[c - 1] = sheet.Cells[r, c].Text.Trim();
            }

            if (values.All(string.IsNullOrEmpty))
            {
                continue;
            }

            result.Add(map(values));
        }

        return result;
    }
}
