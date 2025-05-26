using Base.Enums;
using Base.Models;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Base.Services
{
    public static class _Date
    {        

        #region get difference
        /// <summary>
        /// 2 date difference
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns>days difference, return 0 if same</returns>
        public static int DayDiff(DateTime? start, DateTime? end)
        {
            return (start == null || end == null || start == end)
                ? 0
                : (end.Value - start.Value).Days;
        }

        public static long MinDiff(DateTime? start, DateTime? end)
        {
            return (start == null || end == null || start == end)
                ? 0
                : (end.Value - start.Value).Minutes;
        }
        /// <summary>
        /// 2 string date difference
        /// </summary>
        /// <param name="start">date string</param>
        /// <param name="end">date string</param>
        /// <returns></returns>
        public static int StrDayDiff(ref string error, string start, string end)
        {
            error = ""; //initial
            if (start == end)
                return 0;

            try
            {
                return DayDiff(CsToDt(start), CsToDt(end));
            }
            catch(Exception ex)
            {
                error = "_Date.StrDayDiff() failed: " + ex.Message;
                return 0;
            }
        }

        /// <summary>
        /// 2 min second difference
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static double MiniSecDiff(DateTime? start, DateTime? end)
        {
            return (start == null || end == null)
                ? 0
                : (end.Value - start.Value).TotalMilliseconds;
        }

        //2 min second string difference
        public static double StrMiniSecDiff(ref string error, string start, string end)
        {
            error = ""; //initial
            if (start == end)
                return 0;

            try
            {
                return MiniSecDiff(CsToDt(start), CsToDt(end));
            }
            catch (Exception ex)
            {
                error = "_Date.StrMiniSecDiff() failed: " + ex.Message;
                return 0;
            }
        }
        #endregion
        
        #region check & compare
        //is dates same
        public static bool IsSameDate(DateTime start, DateTime end)
        {
            return (start.Date == end.Date);
        }

        //is string a date or not
        public static bool IsDate(string date)
        {
            //DateTime date2;
            return DateTime.TryParse(date, out var date2);
        }

        /// <summary>
        /// check first 2 dates are inside last 2 dates or not
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="start2"></param>
        /// <param name="end2"></param>
        /// <returns></returns>
        public static bool IsInRange4(DateTime? start, DateTime? end, DateTime start2, DateTime end2)
        {
            return 
                (start == null && end == null) ? true :
                (start != null && end != null) ? (start <= end2 && end >= start2) :
                (start != null && start <= end2) ? true : 
                (end != null && end >= start2);
        }

        /// <summary>
        /// is today between input start/end string 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static bool IsNowInRange(string start, string end)
        {
            var start2 = CsToDate(start);
            if (start2 == null)
                return false;
            var end2 = CsToDate(end);
            if (end2 == null)
                return false;

            return IsNowInRange(start2.Value, end2.Value);
        }
        public static bool IsNowInRange(DateTime start, DateTime end)
        {
            var now = DateTime.Now;
            return (start <= now && end >= now);
        }
        #endregion

        #region get now/today
        /*
        public static long NowTn()
        {
            return Convert.ToInt64(DateTime.Now.ToString("yyyyMMddHHmmss"));
        }

        public static string NowHms6()
        {
            //return Convert.ToInt32(DateTime.Now.ToString("HHmmss"));
            return DateTime.Now.ToString("HHmmss");
        }
        */

        /// <summary>
        /// count second from 1970/1/1 (unix time)
        /// </summary>
        /// <returns></returns>
        public static int NowSec()
        {
            var ts = DateTime.UtcNow - new DateTime(1970, 1, 1);
            return (int)ts.TotalSeconds;
        }

        /// <summary>
        /// get now string(yyyyMMdd_HHmmss) for file name
        /// </summary>
        /// <returns></returns>
        public static string NowSecStr()
        {
            return DateTime.Now.ToString("yyyyMMdd_HHmmss");
        }

        public static string NowYm()
        {
            return DateTime.Now.ToString("yyyyMM");
        }

        public static DateTime Today()
        {
            return DateTime.Today;
        }

        /// <summary>
        /// now db datetime string
        /// </summary>
        /// <returns></returns>
        public static string NowDbStr()
        {
            return ToDbStr(DateTime.Now);
        }

        //for c# & form display
        public static string NowCsStr()
        {
            return ToCsStr(DateTime.Now);
        }

        /*
        //today string
        public static string TodayStr()
        {
            //"yyyy/MM/dd"
            return DateTime.Now.ToString(BackDateFormat);
        }
 
        public static int TodayDn()
        {
            return Convert.ToInt32(DateTime.Now.ToString("yyyyMMdd"));
        }
        */
        #endregion

        #region date convert
        /// <summary>
        /// get dt string for write db
        /// TODO: check other database engine
        /// oracle: TO_CHAR(date_of_birth, 'DD/MON/YYYY HH24:MI:SS')
        /// </summary>
        /// <returns>datetime string</returns>
        public static string ToDbStr(DateTime dt)
        {
            return dt.ToString(_Fun.DbDtFmt);
            /*
            var dbType = _Fun.DbType;
            return (dbType == DbTypeEnum.MSSql || dbType == DbTypeEnum.MySql)
                ? dt.ToString(_Fun.DbDtFmt)
                : "";
            */
        }

        //for c# & form display
        public static string ToCsStr(DateTime dt)
        {
            return dt.ToString(_Fun.CsDtFmt);
        }

        public static string ToDbDateStr(DateTime dt)
        {
            return dt.ToString(_Fun.DbDateFmt);
        }

        /// <summary>
        /// time string(hh:mm) add minute
        /// </summary>
        /// <param name="hm5">time string(hh:mm)</param>
        /// <param name="addMin">minute</param>
        /// <returns>new string (hh:mm)</returns>
        public static string Hm5AddMin(string hm5, int addMin)
        {
            var pos = hm5.IndexOf(":");
            if (pos <= 0)
                return hm5;

            var hour = Convert.ToInt32(hm5[..pos]);
            var min = Convert.ToInt32(hm5[(pos + 1)..]) + addMin;
            if (min < 0)
            {
                hour--;
                min += 60;
            }
            else if (min >= 60)
            {
                hour++;
                min -= 60;
            }
            //return tn_hour.ToString("00") + ":" + tn_min.ToString("00");
            return HmToHm5(hour, min);
        }

        //to HH:mm string
        public static string ToHm5(DateTime dt)
        {
            return dt.ToString("HH:mm");
        }

        /// <summary>
        /// hour min to hm string (hh:mm)
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="min"></param>
        /// <returns></returns>
        public static string HmToHm5(int hour, int min)
        {
            return hour.ToString("00") + ":" + min.ToString("00");
        }

        //birth to age
        public static int BirthToAge(DateTime birth)
        {
            //if (birth == null) return 0;

            var today = DateTime.Today;
            var age = today.Year - birth.Year;
            if (birth > today.AddYears(-age)) age--;
            return age;
        }

        /// <summary>
        /// datetime to tick
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static long ToTick(DateTime dt)
        {
            //if (dt == null) return 0;

            var date0 = new DateTime(1970, 1, 1);
            return (dt.Ticks - date0.Ticks) / 10000000 - 8 * 60 * 60;
        }

        //tick to datetime
        public static DateTime TickToDt(long tick)
        {
            // Unix timestamp is seconds past epoch
            var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return dt.AddSeconds(tick).ToLocalTime();
        }

        /// <summary>
        /// cs datetime string to tick
        /// </summary>
        /// <param name="dts">_Fun.CsDtFormat</param>
        /// <returns></returns>
        public static long CsToTick(string dts)
        {
            var dt = CsToDt(dts);
            return (dt == null) ? 0 : ToTick(dt.Value);
        }

        /// <summary>
        /// cs datetime string to datetime
        /// </summary>
        /// <param name="dts">yyyy/MM/dd hh:mm:ss</param>
        /// <returns></returns>
        public static DateTime? CsToDt(string dts)
        {
            if (dts == "") return null;
            if (dts.Length <= 10) dts += " 00:00:00";

            DateTime.TryParseExact(dts, _Fun.CsDtFmt, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt2);
            return dt2;
        }

        //string to date
        public static DateTime? CsToDate(string dts)
        {
            var dt = CsToDt(dts);
            return (dt == null)
                ? null : dt.Value.Date;
        }

        public static string GetDtStr(DateTime dt)
        {
            return dt.ToString(_Fun.CsDtFmt);
        }

        //no sec
        public static string GetDtStr2(DateTime dt)
        {
            return dt.ToString(_Fun.CsDtFmt2);
        }

        public static string GetDtStr3(DateTime dt)
        {
            return dt.ToString("yyyy/MM/dd HH:mm:ss-fff");
        }

        /// <summary>
        /// get date part string
        /// </summary>
        /// <param name="dts">any format of datetime string</param>
        /// <returns></returns>
        public static string GetDateStr(string dts)
        {
            if (dts == "") return "";
            var pos = dts.IndexOf(" ");
            return (pos <= 0) ? dts : dts[..pos];
        }

        /*
        //??
        public static string Ymd8ToDateStr(string ymd)
        {
            return (ymd.Length == 8)
                ? ymd.Substring(0, 4) + "/" + ymd.Substring(4, 2) + "/" + ymd.Substring(6, 2)
                : "";
        }
        */
        #endregion

        #region chinese date
        /// <summary>
        /// to chinese date string
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="type">1(年月日), 2(/), 3(無分隔符號)</param>
        /// <returns></returns>
        public static string ToTwDateStr(DateTime dt, int type)
        {
            var year = (dt.Year - 1911).ToString();
            return type switch
            {
                1 => year + "年" + dt.Month + "月" + dt.Day + "日",
                2 => year + "/" + dt.Month.ToString("00") + "/" + dt.Day.ToString("00"),
                3 => year + dt.Month.ToString("00") + dt.Day.ToString("00"),
                _ => "??",
            };
        }

        public static DateTime YmdToDate(string ymd)
        {
            return DateTime.ParseExact(ymd, "yyyyMMdd", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// ym 轉換為民國年月
        /// </summary>
        /// <param name="ym">yyyy/mm</param>
        /// <param name="type">1(年月日), 2(/), 3(無分隔符號)</param>
        /// <returns></returns>
        public static string Ym7ToTwYm(string ym, int type)
        {
            if (ym == "") return "";
            var sep = ym.IndexOf("/");
            if (sep <= 0) return "";

            var year = Convert.ToInt32(ym[..sep]) - 1911;
            var month = Convert.ToInt32(ym[(sep + 1)..]);
            return type switch
            {
                1 => year + "年" + month + "月",
                2 => year + "/" + month.ToString("00"),
                3 => year + month.ToString("00"),
                _ => "??",
            };
        }

        //傳回民國年月日
        public static string StrToTwDateStr(string dts, int type)
        {
            return (dts == "")
                ? "" : ToTwDateStr(DateTime.Parse(dts), type);
        }

        /// <summary>
        /// 民國日期(yyymmdd) to 日期字串
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        public static string TwDate7ToDateStr(string ds)
        {
            var len = ds.Length;
            return len switch
            {
                6 => (Convert.ToInt32(ds[..2]) + 1911) + "/" + ds.Substring(2, 2) + "/" + ds.Substring(4, 2),
                7 => (Convert.ToInt32(ds[..3]) + 1911) + "/" + ds.Substring(3, 2) + "/" + ds.Substring(5, 2),
                _ => "",
            };
        }
        #endregion

        #region hour/minute source list
        public static List<IdStrDto> GetHourList()
        {
            var rows = new List<IdStrDto>();
            for(var i=0; i<23; i++)
            {
                rows.Add(new IdStrDto() {
                    Id = i.ToString(),
                    Str = i.ToString(),
                });
            }
            return rows;
        }
        public static List<IdStrDto> GetMinuteList(int step)
        {
            var rows = new List<IdStrDto>();
            for (var i = 0; i < 60; i+=step)
            {
                rows.Add(new IdStrDto()
                {
                    Id = i.ToString(),
                    Str = i.ToString(),
                });
            }
            return rows;
        }
        #endregion

    }//class
}