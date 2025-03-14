using System; // System 6.0.0 - Core .NET functionality
using System.Globalization; // System.Globalization 6.0.0 - For culture-specific date formatting
using VatFilingPricingTool.Web.Utils.Constants; // For date and time format constants

namespace VatFilingPricingTool.Web.Utils
{
    /// <summary>
    /// Provides extension methods for DateTime objects to enhance date and time handling capabilities
    /// in the VAT Filing Pricing Tool web application.
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Formats a DateTime as a short date string using the current culture.
        /// </summary>
        /// <param name="dateTime">The DateTime to format.</param>
        /// <returns>Formatted short date string.</returns>
        public static string ToShortDateString(this DateTime dateTime)
        {
            return dateTime.ToString("d", CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Formats a DateTime as a long date string using the current culture.
        /// </summary>
        /// <param name="dateTime">The DateTime to format.</param>
        /// <returns>Formatted long date string.</returns>
        public static string ToLongDateString(this DateTime dateTime)
        {
            return dateTime.ToString("D", CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Formats a DateTime as a short time string using the current culture.
        /// </summary>
        /// <param name="dateTime">The DateTime to format.</param>
        /// <returns>Formatted short time string.</returns>
        public static string ToShortTimeString(this DateTime dateTime)
        {
            return dateTime.ToString("t", CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Formats a DateTime as a long time string using the current culture.
        /// </summary>
        /// <param name="dateTime">The DateTime to format.</param>
        /// <returns>Formatted long time string.</returns>
        public static string ToLongTimeString(this DateTime dateTime)
        {
            return dateTime.ToString("T", CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Formats a DateTime as a full date and time string.
        /// </summary>
        /// <param name="dateTime">The DateTime to format.</param>
        /// <returns>Formatted full date and time string.</returns>
        public static string ToFullDateTimeString(this DateTime dateTime)
        {
            // Using the DateTimeFormats.FullDateTime constant
            return dateTime.ToString(DateTimeFormats.FullDateTime, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Formats a DateTime as an ISO 8601 date string (yyyy-MM-dd).
        /// </summary>
        /// <param name="dateTime">The DateTime to format.</param>
        /// <returns>ISO 8601 formatted date string.</returns>
        public static string ToIsoDateString(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Formats a DateTime as an ISO 8601 date and time string (yyyy-MM-ddTHH:mm:ss).
        /// </summary>
        /// <param name="dateTime">The DateTime to format.</param>
        /// <returns>ISO 8601 formatted date and time string.</returns>
        public static string ToIsoDateTimeString(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Formats a DateTime as a user-friendly relative date string (Today, Yesterday, etc.).
        /// </summary>
        /// <param name="dateTime">The DateTime to format.</param>
        /// <returns>User-friendly date string.</returns>
        public static string ToFriendlyDateString(this DateTime dateTime)
        {
            DateTime today = DateTime.Today;
            DateTime date = dateTime.Date;

            if (date == today)
                return "Today";
            
            if (date == today.AddDays(-1))
                return "Yesterday";
            
            if (date == today.AddDays(1))
                return "Tomorrow";
            
            // Check if the date is within the last 7 days
            if (date > today.AddDays(-7) && date < today)
                return "Last " + dateTime.ToString("dddd");
            
            // Check if the date is within the next 7 days
            if (date < today.AddDays(7) && date > today)
                return dateTime.ToString("dddd");
            
            // For dates outside the 7-day window, return the short date
            return dateTime.ToShortDateString();
        }

        /// <summary>
        /// Formats a DateTime as a user-friendly relative time string (5 minutes ago, 2 hours ago, etc.).
        /// </summary>
        /// <param name="dateTime">The DateTime to format.</param>
        /// <returns>User-friendly time ago string.</returns>
        public static string ToFriendlyTimeAgoString(this DateTime dateTime)
        {
            TimeSpan timeSpan = DateTime.Now - dateTime;

            if (timeSpan < TimeSpan.Zero)
                return "in the future";

            if (timeSpan.TotalMinutes < 1)
                return "just now";
            
            if (timeSpan.TotalHours < 1)
                return $"{(int)timeSpan.TotalMinutes} minute{(timeSpan.TotalMinutes == 1 ? "" : "s")} ago";
            
            if (timeSpan.TotalDays < 1)
                return $"{(int)timeSpan.TotalHours} hour{(timeSpan.TotalHours == 1 ? "" : "s")} ago";
            
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} day{(timeSpan.TotalDays == 1 ? "" : "s")} ago";
            
            if (timeSpan.TotalDays < 30)
            {
                int weeks = (int)(timeSpan.TotalDays / 7);
                return $"{weeks} week{(weeks == 1 ? "" : "s")} ago";
            }
            
            if (timeSpan.TotalDays < 365)
            {
                int months = (int)(timeSpan.TotalDays / 30);
                return $"{months} month{(months == 1 ? "" : "s")} ago";
            }
            
            int years = (int)(timeSpan.TotalDays / 365);
            return $"{years} year{(years == 1 ? "" : "s")} ago";
        }

        /// <summary>
        /// Formats a DateTime as a quarter string (e.g., 'Q1 2023').
        /// </summary>
        /// <param name="dateTime">The DateTime to format.</param>
        /// <returns>Quarter string representation.</returns>
        public static string ToQuarterString(this DateTime dateTime)
        {
            int quarter = GetQuarter(dateTime);
            return $"Q{quarter} {dateTime.Year}";
        }

        /// <summary>
        /// Gets the quarter (1-4) for a given DateTime.
        /// </summary>
        /// <param name="dateTime">The DateTime to get the quarter for.</param>
        /// <returns>Quarter number (1-4).</returns>
        public static int GetQuarter(this DateTime dateTime)
        {
            return (dateTime.Month - 1) / 3 + 1;
        }

        /// <summary>
        /// Returns a new DateTime representing the start of the day (00:00:00).
        /// </summary>
        /// <param name="dateTime">The DateTime to get the start of day for.</param>
        /// <returns>DateTime at start of day.</returns>
        public static DateTime StartOfDay(this DateTime dateTime)
        {
            return dateTime.Date;
        }

        /// <summary>
        /// Returns a new DateTime representing the end of the day (23:59:59.999).
        /// </summary>
        /// <param name="dateTime">The DateTime to get the end of day for.</param>
        /// <returns>DateTime at end of day.</returns>
        public static DateTime EndOfDay(this DateTime dateTime)
        {
            return dateTime.Date.AddDays(1).AddMilliseconds(-1);
        }

        /// <summary>
        /// Returns a new DateTime representing the start of the month.
        /// </summary>
        /// <param name="dateTime">The DateTime to get the start of month for.</param>
        /// <returns>DateTime at start of month.</returns>
        public static DateTime StartOfMonth(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, 1);
        }

        /// <summary>
        /// Returns a new DateTime representing the end of the month.
        /// </summary>
        /// <param name="dateTime">The DateTime to get the end of month for.</param>
        /// <returns>DateTime at end of month.</returns>
        public static DateTime EndOfMonth(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, DateTime.DaysInMonth(dateTime.Year, dateTime.Month))
                .EndOfDay();
        }

        /// <summary>
        /// Returns a new DateTime representing the start of the quarter.
        /// </summary>
        /// <param name="dateTime">The DateTime to get the start of quarter for.</param>
        /// <returns>DateTime at start of quarter.</returns>
        public static DateTime StartOfQuarter(this DateTime dateTime)
        {
            int quarter = GetQuarter(dateTime);
            int firstMonthOfQuarter = (quarter - 1) * 3 + 1;
            return new DateTime(dateTime.Year, firstMonthOfQuarter, 1);
        }

        /// <summary>
        /// Returns a new DateTime representing the end of the quarter.
        /// </summary>
        /// <param name="dateTime">The DateTime to get the end of quarter for.</param>
        /// <returns>DateTime at end of quarter.</returns>
        public static DateTime EndOfQuarter(this DateTime dateTime)
        {
            int quarter = GetQuarter(dateTime);
            int lastMonthOfQuarter = quarter * 3;
            return new DateTime(dateTime.Year, lastMonthOfQuarter, DateTime.DaysInMonth(dateTime.Year, lastMonthOfQuarter))
                .EndOfDay();
        }

        /// <summary>
        /// Returns a new DateTime representing the start of the year.
        /// </summary>
        /// <param name="dateTime">The DateTime to get the start of year for.</param>
        /// <returns>DateTime at start of year.</returns>
        public static DateTime StartOfYear(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, 1, 1);
        }

        /// <summary>
        /// Returns a new DateTime representing the end of the year.
        /// </summary>
        /// <param name="dateTime">The DateTime to get the end of year for.</param>
        /// <returns>DateTime at end of year.</returns>
        public static DateTime EndOfYear(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, 12, 31).EndOfDay();
        }

        /// <summary>
        /// Determines if the DateTime falls on a weekend (Saturday or Sunday).
        /// </summary>
        /// <param name="dateTime">The DateTime to check.</param>
        /// <returns>True if the date is a weekend, otherwise false.</returns>
        public static bool IsWeekend(this DateTime dateTime)
        {
            return dateTime.DayOfWeek == DayOfWeek.Saturday || dateTime.DayOfWeek == DayOfWeek.Sunday;
        }

        /// <summary>
        /// Determines if the DateTime falls on a workday (Monday through Friday).
        /// </summary>
        /// <param name="dateTime">The DateTime to check.</param>
        /// <returns>True if the date is a workday, otherwise false.</returns>
        public static bool IsWorkDay(this DateTime dateTime)
        {
            return !dateTime.IsWeekend();
        }

        /// <summary>
        /// Converts a DateTime to a JavaScript-compatible date string.
        /// </summary>
        /// <param name="dateTime">The DateTime to convert.</param>
        /// <returns>JavaScript-compatible date string.</returns>
        public static string ToJsDate(this DateTime dateTime)
        {
            return dateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
        }
    }
}