namespace FleetApi.Helpers
{
    public static class FleetHelper
    {
        public static string CleanLicensePlate(string plate)
        {
            if (string.IsNullOrWhiteSpace(plate))
                return string.Empty;

            // Sanitizes hyphenation and whitespace
            return plate.Replace("-", "").Trim().ToLower();
        }

        public static bool IsValidYear(int year)
        {
            int currentYear = DateTime.Now.Year;
            return year >= 1900 && year <= currentYear + 1;
        }
    }
}