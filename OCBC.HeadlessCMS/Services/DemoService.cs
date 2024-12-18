using System.Globalization;
using Microsoft.Extensions.Localization;

namespace OCBC.HeadlessCMS.Services;

public class DemoService(IStringLocalizer<DemoService> stringLocalizer) : IDemoService
{
    public object GetDemoTranslatedText()
    {
        // Hardcode the locale for testing
        var testCulture = "ms-SG";
        CultureInfo.CurrentCulture = new CultureInfo(testCulture);
        CultureInfo.CurrentUICulture = new CultureInfo(testCulture);

        // Use the localizer to get a string
        var localizedString = stringLocalizer["weather_Hot"];

        // Return the localized string
        return new { Locale = testCulture, Translation = localizedString };
    }
}