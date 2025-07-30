using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using GeneXus.Application;
using Xunit;

namespace DotNetCoreUnitTest.Helpers
{
	public class GXRestAPIClientTest
	{
		[Fact]
		public void TestAddBodyVarWithDecimalUsesCultureInvariant()
		{
			// Save the current culture
			CultureInfo originalCulture = Thread.CurrentThread.CurrentCulture;
			CultureInfo originalUICulture = Thread.CurrentThread.CurrentUICulture;

			try
			{
				// Set culture to Spanish (which uses comma as decimal separator)
				CultureInfo spanishCulture = new CultureInfo("es-ES");
				Thread.CurrentThread.CurrentCulture = spanishCulture;
				Thread.CurrentThread.CurrentUICulture = spanishCulture;

				// Verify that current culture actually uses comma as decimal separator
				decimal testValue = 1.23m;
				string formattedWithCurrentCulture = testValue.ToString();
				Assert.Contains(",", formattedWithCurrentCulture, StringComparison.OrdinalIgnoreCase); // Confirms comma is used in current culture

				GXRestAPIClient client = new GXRestAPIClient();
				client.AddBodyVar("testDecimal", testValue);

				// Verify that the value in the body uses dot as decimal separator (invariant culture)
				string storedValue = client.BodyVars["testDecimal"];
				Assert.Contains(".", storedValue, StringComparison.OrdinalIgnoreCase);
				Assert.DoesNotContain(",", storedValue, StringComparison.OrdinalIgnoreCase);
				Assert.Equal("1.23", storedValue); // Should be exactly "1.23"
			}
			finally
			{
				// Restore original culture
				Thread.CurrentThread.CurrentCulture = originalCulture;
				Thread.CurrentThread.CurrentUICulture = originalUICulture;
			}
		}

		[Fact]
		public void TestGetBodyNumReadsDecimalCorrectlyFromResponseData()
		{
			// Save the current culture
			CultureInfo originalCulture = Thread.CurrentThread.CurrentCulture;
			CultureInfo originalUICulture = Thread.CurrentThread.CurrentUICulture;

			try
			{
				// Set culture to Spanish (which uses comma as decimal separator)
				CultureInfo spanishCulture = new CultureInfo("es-ES");
				Thread.CurrentThread.CurrentCulture = spanishCulture;
				Thread.CurrentThread.CurrentUICulture = spanishCulture;

				// Sample JSON with numeric values
				string jsonResponse = @"{
                                ""Num4DigitOut"":32767,
                                ""Num5con1decOut"":155.12346,
                                ""Num8DigitOut"":""42949672"",
                                ""Num10con2decOut"":""1551234.56"",
                                ""Num18DigitOut"":""184467440737095599""
                        }";

				GXRestAPIClient client = new GXRestAPIClient();
				client.ResponseData = GeneXus.Utils.RestAPIHelpers.ReadRestParameters(jsonResponse);

				// Verify that GetBodyNum correctly reads the decimal value
				decimal decimalValue = client.GetBodyNum("Num5con1decOut");

				// Verify the correct value was obtained
				Assert.Equal(155.12346m, decimalValue);

				// Verify that output format uses invariant culture (decimal point)
				string formattedValue = decimalValue.ToString(CultureInfo.InvariantCulture);
				Assert.Contains(".", formattedValue, StringComparison.OrdinalIgnoreCase);
				Assert.DoesNotContain(",", formattedValue, StringComparison.OrdinalIgnoreCase);

				// Also verify that formatting with current culture uses comma
				string formattedWithCurrentCulture = decimalValue.ToString();
				Assert.Contains(",", formattedWithCurrentCulture, StringComparison.OrdinalIgnoreCase);
			}
			finally
			{
				// Restore original culture
				Thread.CurrentThread.CurrentCulture = originalCulture;
				Thread.CurrentThread.CurrentUICulture = originalUICulture;
			}
		}

		[Fact]
		public void TestOptimizedGetBodyNumDirectlyUsesNumericTypes()
		{
			// Test that the optimized version of GetBodyNum directly uses numeric values

			// Create a dictionary with different numeric data types
			var responseData = new Dictionary<string, object>
				{
						{ "decimalvalue", 123.45m },           // Decimal value
                        { "doublevalue", 678.90 },             // Double value
                        { "intvalue", 12345 },                 // Integer value
                        { "longvalue", 9876543210L },          // Long value
                        { "shortvalue", (short)42 },           // Short value
                        { "floatvalue", 3.14159f },            // Float value
                        { "stringvalue", "987.654" }           // String value
                };

			// Create instance and assign data
			GXRestAPIClient client = new GXRestAPIClient();
			client.ResponseData = responseData;

			// Verify that each type is processed correctly without unnecessary conversions
			Assert.Equal(123.45m, client.GetBodyNum("decimalValue"));
			Assert.Equal(678.90m, client.GetBodyNum("doubleValue"));
			Assert.Equal(12345m, client.GetBodyNum("intValue"));
			Assert.Equal(9876543210m, client.GetBodyNum("longValue"));
			Assert.Equal(42m, client.GetBodyNum("shortValue"));
			Assert.Equal(3.14159m, client.GetBodyNum("floatValue"));
			Assert.Equal(987.654m, client.GetBodyNum("stringValue"));
		}
	}
}