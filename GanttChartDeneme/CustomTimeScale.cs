using System;
using System.Collections.Generic;
using System.Text;
using DevExpress.XtraScheduler;

namespace GanttChartDeneme
{
    #region #customscales
    public class CustomTimeScale15Minutes : TimeScale15Minutes
    {
        private TimeSpan StartHour = TimeSpan.Parse("08:00");
        private TimeSpan FinishHour = TimeSpan.Parse("19:00");

        protected override TimeSpan SortingWeight
        {
            get { return TimeSpan.FromMinutes(15); }
        }

        public override DateTime Floor(DateTime date)
        {
            DateTime time;

            if (date == DateTime.MinValue || date == DateTime.MaxValue)
            {
                time = RoundToMinute(date, date.Hour, date.Minute);
                return time;
            }
            else if (date.TimeOfDay < StartHour)
            {
                time = RoundToMinute(date.AddDays(-1), FinishHour.Hours, 45);
                return time;
            }
            else
            {
                if (date.Minute >= 45)
                {
                    time = RoundToMinute(date, date.Hour, 45);
                    return time;
                }
                else if (date.Minute >= 30 && date.Minute < 45)
                {
                    time = RoundToMinute(date, date.Hour, 30);
                    return time;
                }
                else if (date.Minute >= 15 && date.Minute < 30)
                {
                    time = RoundToMinute(date, date.Hour, 15);
                    return time;
                }
                else
                {
                    time = RoundToMinute(date, date.Hour, 0);
                    return time;
                }
            }
        }

        protected DateTime RoundToMinute(DateTime date, int hour, int minute)
        {
            return new DateTime(date.Year, date.Month, date.Day, hour, minute, 0);
        }
        protected override bool HasNextDate(DateTime date)
        {
            return date <= RoundToMinute(DateTime.MaxValue, FinishHour.Hours, 0);
        }
        public override DateTime GetNextDate(DateTime date)
        {
            return (date >= Convert.ToDateTime(date.ToShortDateString() + " " + FinishHour.Hours + ":45")) ? RoundToMinute(date.AddDays(1), StartHour.Hours, 0) : date.AddMinutes(15);
        }
    }
    public class CustomTimeScaleDay : TimeScaleDay
    {
        public override DateTime Floor(DateTime date)
        {
            if (date == DateTime.MinValue)
                return date.AddHours(8);

            DateTime start = base.Floor(date);
            if (date.Hour < 8)
                start = start.AddDays(-1);

            return start.AddHours(8);
        }
    }
    public class CustomTimeScaleHour : TimeScale
    {
        private const int StartHour = 8;
        private const int FinishHour = 19;

        protected override string DefaultDisplayFormat { get { return "HH:mm"; } }
        protected override string DefaultMenuCaption { get { return "8:00-19:00"; } }

        protected override TimeSpan SortingWeight
        {
            get { return TimeSpan.FromHours(FinishHour - StartHour + 1); }
        }
        public override DateTime Floor(DateTime date)
        {
            if (date == DateTime.MinValue || date == DateTime.MaxValue)
                return RoundToHour(date, date.Hour);

            if (date.Hour < StartHour)
                // Round down to the end of the previous working day.
                return RoundToHour(date.AddDays(-1), FinishHour);

            if (date.Hour > FinishHour)
                // Round down to the end of the current working day.
                return RoundToHour(date, FinishHour);

            return RoundToHour(date, date.Hour);
        }
        protected DateTime RoundToHour(DateTime date, int hour)
        {
            return new DateTime(date.Year, date.Month, date.Day, hour, 0, 0);
        }
        protected override bool HasNextDate(DateTime date)
        {
            return date <= RoundToHour(DateTime.MaxValue, FinishHour);
        }
        public override DateTime GetNextDate(DateTime date)
        {
            return (date.Hour > FinishHour - 1) ? RoundToHour(date.AddDays(1), StartHour) : date.AddHours(1);
        }
    }
    #endregion #customscales
}
