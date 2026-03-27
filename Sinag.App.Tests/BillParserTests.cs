using Sinag.App.Features.Scan;
using Sinag.App.Models;

namespace Sinag.App.Tests;

public class BillParserTests
{
    private readonly BillParser _parser = new();

    private const string SampleDlpcBillText = @"
DAVAO LIGHT AND POWER COMPANY
Account No: 12345678
Customer Name: JUAN DELA CRUZ

Billing Period: Feb 26 - Mar 25, 2026
Rate Class: Residential

kWh    350

Generation Charge          6.52
Transmission Charge        1.23
Distribution Charge        2.45

Total Amount Due    3,542.50
";

    [Fact]
    public void Parse_ValidDlpcBill_ExtractsAllFields()
    {
        var result = _parser.Parse(SampleDlpcBillText);

        Assert.True(result.IsDlpcBill);
        Assert.Equal(350, result.KwhConsumed);
        Assert.Equal(3542.50m, result.TotalAmountDue);
        Assert.Equal(6.52m, result.GenerationChargePerKwh);
    }

    [Fact]
    public void Parse_ValidDlpcBill_SetsHighConfidenceForKwh()
    {
        var result = _parser.Parse(SampleDlpcBillText);

        Assert.Equal(ConfidenceLevel.High, result.FieldConfidence["KwhConsumed"]);
    }

    [Fact]
    public void Parse_NonDlpcBill_ThrowsBillParseException()
    {
        var nonDlpcText = "MERALCO Electric Company\nTotal Amount Due 5000.00\nkWh 450";

        var ex = Assert.Throws<BillParseException>(() => _parser.Parse(nonDlpcText));
        Assert.Equal("Not a DLPC bill", ex.Message);
    }

    [Fact]
    public void Parse_EmptyText_ThrowsBillParseException()
    {
        Assert.Throws<BillParseException>(() => _parser.Parse(""));
    }

    [Fact]
    public void Parse_NullText_ThrowsBillParseException()
    {
        Assert.Throws<BillParseException>(() => _parser.Parse(null!));
    }

    [Fact]
    public void Parse_PartialExtraction_SetsLowConfidenceForMissingFields()
    {
        var partialText = "DAVAO LIGHT\nkWh 200\nSome other text";

        var result = _parser.Parse(partialText);

        Assert.True(result.IsDlpcBill);
        Assert.Equal(200, result.KwhConsumed);
        Assert.Equal(ConfidenceLevel.High, result.FieldConfidence["KwhConsumed"]);
        Assert.Equal(ConfidenceLevel.Low, result.FieldConfidence["TotalAmountDue"]);
    }

    [Fact]
    public void Parse_KwhFormatVariant_ExtractsCorrectly()
    {
        var text = "DLPC\n450kWh consumed\nTotal Amount Due 4,200.00";

        var result = _parser.Parse(text);

        Assert.Equal(450, result.KwhConsumed);
    }

    [Fact]
    public void Parse_KwhAfterLabel_ExtractsCorrectly()
    {
        var text = "DAVAO LIGHT\nKWH  275\nTotal 2,800.50";

        var result = _parser.Parse(text);

        Assert.Equal(275, result.KwhConsumed);
    }

    [Fact]
    public void Parse_WithDlpcAnchor_IdentifiesAsDlpcBill()
    {
        var text = "DLPC Account Statement\nkWh 300";

        var result = _parser.Parse(text);

        Assert.True(result.IsDlpcBill);
    }

    [Fact]
    public void Parse_ExtractsBillingMonth()
    {
        var result = _parser.Parse(SampleDlpcBillText);

        // "Feb" or "Mar" should be found — first occurrence is Feb (month 2) or Mar (month 3)
        Assert.True(result.BillingMonth >= 1 && result.BillingMonth <= 12);
    }

    [Fact]
    public void Parse_DefaultGenerationCharge_WhenNotFound()
    {
        var text = "DAVAO LIGHT\nkWh 200\nTotal Amount Due 2,000.00";

        var result = _parser.Parse(text);

        Assert.Equal(6.52m, result.GenerationChargePerKwh);
        Assert.Equal(ConfidenceLevel.Medium, result.FieldConfidence["GenerationChargePerKwh"]);
    }
}
