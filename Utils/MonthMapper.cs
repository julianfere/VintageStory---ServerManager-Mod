using System;
using Vintagestory.API.Common;

namespace ServerManager.Utils
{
    public class MonthMapper
    {
        public static string MapMonthEnumToString(EnumMonth month)
        {
            switch (month)
            {
                case EnumMonth.January:
                    return "January";
                case EnumMonth.February:
                    return "February";
                case EnumMonth.March:
                    return "March";
                case EnumMonth.April:
                    return "April";
                case EnumMonth.May:
                    return "May";
                case EnumMonth.June:
                    return "June";
                case EnumMonth.July:
                    return "July";
                case EnumMonth.August:
                    return "August";
                case EnumMonth.September:
                    return "September";
                case EnumMonth.October:
                    return "October";
                case EnumMonth.November:
                    return "November";
                case EnumMonth.December:
                    return "December";
                default:
                    throw new ArgumentOutOfRangeException(nameof(month), month, null);
            }
        }
    }
}
