using Hangfire;
using System;

namespace StarChef.BackgroundServices.Common
{
    /// <summary>
    /// Wrapper of the Hangfire Cron helper class that provides common values for the cron expressions.
    /// Purpose of the class is to remove the need of refrence to Hangfire library in order to shedule a job.
    /// </summary>
    public static class CronInterval
    {
        /// <summary>Returns cron expression that fires every minute.</summary>
        public static string Minutely()
        {
            return Cron.Minutely();
        }

        /// <summary>
        /// Returns cron expression that fires every hour at the first minute.
        /// </summary>
        public static string Hourly()
        {
            return Cron.Hourly();
        }

        /// <summary>
        /// Returns cron expression that fires every hour at the specified minute.
        /// </summary>
        /// <param name="minute">The minute in which the schedule will be activated (0-59).</param>
        public static string Hourly(int minute)
        {
            return Cron.Hourly(minute);
        }

        /// <summary>
        /// Returns cron expression that fires every day at 00:00 UTC.
        /// </summary>
        public static string Daily()
        {
            return Cron.Daily();
        }

        /// <summary>
        /// Returns cron expression that fires every day at the first minute of
        /// the specified hour in UTC.
        /// </summary>
        /// <param name="hour">The hour in which the schedule will be activated (0-23).</param>
        public static string Daily(int hour)
        {
            return Cron.Daily(hour);
        }

        /// <summary>
        /// Returns cron expression that fires every day at the specified hour and minute
        /// in UTC.
        /// </summary>
        /// <param name="hour">The hour in which the schedule will be activated (0-23).</param>
        /// <param name="minute">The minute in which the schedule will be activated (0-59).</param>
        public static string Daily(int hour, int minute)
        {
            return Cron.Daily(hour, minute);
        }

        /// <summary>
        /// Returns cron expression that fires every week at Monday, 00:00 UTC.
        /// </summary>
        public static string Weekly()
        {
            return Cron.Weekly();
        }

        /// <summary>
        /// Returns cron expression that fires every week at 00:00 UTC of the specified
        /// day of the week.
        /// </summary>
        /// <param name="dayOfWeek">The day of week in which the schedule will be activated.</param>
        public static string Weekly(DayOfWeek dayOfWeek)
        {
            return Cron.Weekly(dayOfWeek);
        }

        /// <summary>
        /// Returns cron expression that fires every week at the first minute
        /// of the specified day of week and hour in UTC.
        /// </summary>
        /// <param name="dayOfWeek">The day of week in which the schedule will be activated.</param>
        /// <param name="hour">The hour in which the schedule will be activated (0-23).</param>
        public static string Weekly(DayOfWeek dayOfWeek, int hour)
        {
            return Cron.Weekly(dayOfWeek, hour);
        }

        /// <summary>
        /// Returns cron expression that fires every week at the specified day
        /// of week, hour and minute in UTC.
        /// </summary>
        /// <param name="dayOfWeek">The day of week in which the schedule will be activated.</param>
        /// <param name="hour">The hour in which the schedule will be activated (0-23).</param>
        /// <param name="minute">The minute in which the schedule will be activated (0-59).</param>
        public static string Weekly(DayOfWeek dayOfWeek, int hour, int minute)
        {
            return Cron.Weekly(dayOfWeek, hour, minute);
        }

        /// <summary>
        /// Returns cron expression that fires every month at 00:00 UTC of the first
        /// day of month.
        /// </summary>
        public static string Monthly()
        {
            return Cron.Monthly();
        }

        /// <summary>
        /// Returns cron expression that fires every month at 00:00 UTC of the specified
        /// day of month.
        /// </summary>
        /// <param name="day">The day of month in which the schedule will be activated (1-31).</param>
        public static string Monthly(int day)
        {
            return Cron.Monthly(day);
        }

        /// <summary>
        /// Returns cron expression that fires every month at the first minute of the
        /// specified day of month and hour in UTC.
        /// </summary>
        /// <param name="day">The day of month in which the schedule will be activated (1-31).</param>
        /// <param name="hour">The hour in which the schedule will be activated (0-23).</param>
        public static string Monthly(int day, int hour)
        {
            return Cron.Monthly(day, hour);
        }

        /// <summary>
        /// Returns cron expression that fires every month at the specified day of month,
        /// hour and minute in UTC.
        /// </summary>
        /// <param name="day">The day of month in which the schedule will be activated (1-31).</param>
        /// <param name="hour">The hour in which the schedule will be activated (0-23).</param>
        /// <param name="minute">The minute in which the schedule will be activated (0-59).</param>
        public static string Monthly(int day, int hour, int minute)
        {
            return Cron.Monthly(day, hour, minute);
        }

        /// <summary>
        /// Returns cron expression that fires every year on Jan, 1st at 00:00 UTC.
        /// </summary>
        public static string Yearly()
        {
            return Cron.Yearly();
        }

        /// <summary>
        /// Returns cron expression that fires every year in the first day at 00:00 UTC
        /// of the specified month.
        /// </summary>
        /// <param name="month">The month in which the schedule will be activated (1-12).</param>
        public static string Yearly(int month)
        {
            return Cron.Yearly(month);
        }

        /// <summary>
        /// Returns cron expression that fires every year at 00:00 UTC of the specified
        /// month and day of month.
        /// </summary>
        /// <param name="month">The month in which the schedule will be activated (1-12).</param>
        /// <param name="day">The day of month in which the schedule will be activated (1-31).</param>
        public static string Yearly(int month, int day)
        {
            return Cron.Yearly(month, day);
        }

        /// <summary>
        /// Returns cron expression that fires every year at the first minute of the
        /// specified month, day and hour in UTC.
        /// </summary>
        /// <param name="month">The month in which the schedule will be activated (1-12).</param>
        /// <param name="day">The day of month in which the schedule will be activated (1-31).</param>
        /// <param name="hour">The hour in which the schedule will be activated (0-23).</param>
        public static string Yearly(int month, int day, int hour)
        {
            return Cron.Yearly(month, day, hour);
        }

        /// <summary>
        /// Returns cron expression that fires every year at the specified month, day,
        /// hour and minute in UTC.
        /// </summary>
        /// <param name="month">The month in which the schedule will be activated (1-12).</param>
        /// <param name="day">The day of month in which the schedule will be activated (1-31).</param>
        /// <param name="hour">The hour in which the schedule will be activated (0-23).</param>
        /// <param name="minute">The minute in which the schedule will be activated (0-59).</param>
        public static string Yearly(int month, int day, int hour, int minute)
        {
            return Cron.Yearly(month, day, hour, minute);
        }
    }
}
