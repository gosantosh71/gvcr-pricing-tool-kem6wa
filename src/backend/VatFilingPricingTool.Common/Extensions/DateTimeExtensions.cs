using System; // System package version: 6.0.0
using System.Globalization; // System.Globalization package version: 6.0.0
using VatFilingPricingTool.Common.Models;

namespace VatFilingPricingTool.Common.Extensions
{
    /// <summary>
    /// Provides extension methods for DateTime manipulation, formatting, and validation used throughout the VAT Filing Pricing Tool application.
    /// These methods enhance the functionality of the DateTime type with common operations needed for VAT filing date handling.
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Checks if a date is within a specified range.
        /// </summary>
        /// <param name="date">The date to check.</param>
        /// <param name="startDate">The start of the range.</param>
        /// <param name="endDate">The optional end of the range. If null, only checks if date is after startDate.</param>
        /// <returns>True if the date is within the range, otherwise false.</returns>
        public static bool IsInRange(this DateTime date, DateTime startDate, DateTime? endDate = null)
        {
            return date >= startDate && (!endDate.HasValue || date <= endDate.Value);
        }

        /// <summary>
        /// Checks if a date is in the past.
        /// </summary>
        /// <param name="date">The date to check.</param>
        /// <returns>True if the date is in the past, otherwise false.</returns>
        public static bool IsInPast(this DateTime date)
        {
            return date < DateTime.UtcNow;
        }

        /// <summary>
        /// Checks if a date is in the future.
        /// </summary>
        /// <param name="date">The date to check.</param>
        /// <returns>True if the date is in the future, otherwise false.</returns>
        public static bool IsInFuture(this DateTime date)
        {
            return date > DateTime.UtcNow;
        }

        /// <summary>
        /// Converts a DateTime to ISO 8601 date string (yyyy-MM-dd).
        /// </summary>
        /// <param name="date">The date to format.</param>
        /// <returns>The date formatted as an ISO 8601 date string.</returns>
        public static string ToIsoDateString(this DateTime date)
        {
            return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a DateTime to ISO 8601 date and time string (yyyy-MM-ddTHH:mm:ssZ).
        /// </summary>
        /// <param name="dateTime">The date and time to format.</param>
        /// <returns>The date and time formatted as an ISO 8601 string.</returns>
        public static string ToIsoDateTimeString(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Formats a date as a filing period string based on filing frequency (e.g., 'Q1 2023', 'Jan 2023').
        /// </summary>
        /// <param name="date">The date to format.</param>
        /// <param name="frequency">The filing frequency.</param>
        /// <returns>The formatted filing period string.</returns>
        public static string ToFilingPeriodString(this DateTime date, FilingFrequency frequency)
        {
            switch (frequency)
            {
                case FilingFrequency.Monthly:
                    return date.ToString("MMM yyyy", CultureInfo.InvariantCulture);
                case FilingFrequency.Quarterly:
                    int quarter = date.GetQuarter();
                    return $"Q{quarter} {date.Year}";
                case FilingFrequency.Annual:
                    return date.Year.ToString(CultureInfo.InvariantCulture);
                default:
                    throw new ArgumentOutOfRangeException(nameof(frequency), frequency, "Unsupported filing frequency");
            }
        }

        /// <summary>
        /// Gets the quarter (1-4) for a given date.
        /// </summary>
        /// <param name="date">The date to check.</param>
        /// <returns>The quarter (1-4).</returns>
        public static int GetQuarter(this DateTime date)
        {
            return (date.Month - 1) / 3 + 1;
        }

        /// <summary>
        /// Gets the first day of the quarter for a given date.
        /// </summary>
        /// <param name="date">The date to check.</param>
        /// <returns>The first day of the quarter.</returns>
        public static DateTime GetStartOfQuarter(this DateTime date)
        {
            int quarter = date.GetQuarter();
            int firstMonthOfQuarter = (quarter - 1) * 3 + 1;
            return new DateTime(date.Year, firstMonthOfQuarter, 1);
        }

        /// <summary>
        /// Gets the last day of the quarter for a given date.
        /// </summary>
        /// <param name="date">The date to check.</param>
        /// <returns>The last day of the quarter.</returns>
        public static DateTime GetEndOfQuarter(this DateTime date)
        {
            int quarter = date.GetQuarter();
            int lastMonthOfQuarter = quarter * 3;
            return new DateTime(date.Year, lastMonthOfQuarter, 1).AddMonths(1).AddDays(-1);
        }

        /// <summary>
        /// Gets the first day of the month for a given date.
        /// </summary>
        /// <param name="date">The date to check.</param>
        /// <returns>The first day of the month.</returns>
        public static DateTime GetStartOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        /// <summary>
        /// Gets the last day of the month for a given date.
        /// </summary>
        /// <param name="date">The date to check.</param>
        /// <returns>The last day of the month.</returns>
        public static DateTime GetEndOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1).AddMonths(1).AddDays(-1);
        }

        /// <summary>
        /// Gets the first day of the year for a given date.
        /// </summary>
        /// <param name="date">The date to check.</param>
        /// <returns>The first day of the year.</returns>
        public static DateTime GetStartOfYear(this DateTime date)
        {
            return new DateTime(date.Year, 1, 1);
        }

        /// <summary>
        /// Gets the last day of the year for a given date.
        /// </summary>
        /// <param name="date">The date to check.</param>
        /// <returns>The last day of the year.</returns>
        public static DateTime GetEndOfYear(this DateTime date)
        {
            return new DateTime(date.Year, 12, 31);
        }

        /// <summary>
        /// Formats a date in a user-friendly format (e.g., 'Today', 'Yesterday', 'Monday', or 'dd MMM yyyy').
        /// </summary>
        /// <param name="date">The date to format.</param>
        /// <returns>The user-friendly date string.</returns>
        public static string ToFriendlyDateString(this DateTime date)
        {
            DateTime today = DateTime.Today;
            int dayDifference = (today - date.Date).Days;

            if (dayDifference == 0)
                return "Today";
            if (dayDifference == 1)
                return "Yesterday";
            if (dayDifference > 1 && dayDifference < 7)
                return date.ToString("dddd", CultureInfo.InvariantCulture); // Day name (e.g., "Monday")

            return date.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Formats a time in a user-friendly format (e.g., '5 minutes ago', '2 hours ago', or HH:mm).
        /// </summary>
        /// <param name="dateTime">The date and time to format.</param>
        /// <returns>The user-friendly time string.</returns>
        public static string ToFriendlyTimeString(this DateTime dateTime)
        {
            TimeSpan timeSpan = DateTime.UtcNow - dateTime;

            if (timeSpan.TotalMinutes < 1)
                return "Just now";
            if (timeSpan.TotalHours < 1)
                return $"{(int)timeSpan.TotalMinutes} minutes ago";
            if (timeSpan.TotalDays < 1)
                return $"{(int)timeSpan.TotalHours} hours ago";

            return dateTime.ToString("HH:mm", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Adds a specified number of business days (excluding weekends) to a date.
        /// </summary>
        /// <param name="date">The starting date.</param>
        /// <param name="days">The number of business days to add.</param>
        /// <returns>The date with business days added.</returns>
        public static DateTime AddBusinessDays(this DateTime date, int days)
        {
            int businessDaysAdded = 0;
            DateTime resultDate = date;

            while (businessDaysAdded < days)
            {
                resultDate = resultDate.AddDays(1);
                if (resultDate.DayOfWeek != DayOfWeek.Saturday && resultDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    businessDaysAdded++;
                }
            }

            return resultDate;
        }

        /// <summary>
        /// Checks if a date is a business day (not a weekend).
        /// </summary>
        /// <param name="date">The date to check.</param>
        /// <returns>True if the date is a business day, otherwise false.</returns>
        public static bool IsBusinessDay(this DateTime date)
        {
            return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
        }
    }
}