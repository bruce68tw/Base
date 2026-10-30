using Base.Enums;
using Base.Models;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Base.Services
{
    /// <summary>
    /// 日期格式、區間與時間差工具；部分方法使用固定時區或現有非標準計算。
    /// </summary>
    public static class _Date
    {        

        #region get difference
        /// <summary>
        /// 取得時間差的整日分量；任一時間為 null 時回傳 0。
        /// </summary>
        public static int DayDiff(DateTime? start, DateTime? end)
        {
            return (start == null || end == null || start == end)
                ? 0
                : (end.Value - start.Value).Days;
        }

        /// <summary>
        /// 取得 TimeSpan.Minutes 分量，非總分鐘數；任一時間為 null 時回傳 0。
        /// </summary>
        public static long MinDiff(DateTime? start, DateTime? end)
        {
            return (start == null || end == null || start == end)
                ? 0
                : (end.Value - start.Value).Minutes;
        }
        /// <summary>
        /// 解析日期字串後計算整日差；捕捉例外時設定 error 並回傳 0。
        /// </summary>
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
        /// 取得總毫秒差；任一時間為 null 時回傳 0。
        /// </summary>
        public static double MiniSecDiff(DateTime? start, DateTime? end)
        {
            return (start == null || end == null)
                ? 0
                : (end.Value - start.Value).TotalMilliseconds;
        }

        /// <summary>
        /// 解析日期字串並計算毫秒差；捕捉例外時設定 error 並回傳 0。
        /// </summary>
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
        /// <summary>
        /// 比較年月日，忽略時分秒。
        /// </summary>
        public static bool IsSameDate(DateTime start, DateTime end)
        {
            return (start.Date == end.Date);
        }

        /// <summary>
        /// 以 DateTime.TryParse 判斷字串是否可解析。
        /// </summary>
        public static bool IsDate(string date)
        {
            //DateTime date2;
            return DateTime.TryParse(date, out var date2);
        }

        /// <summary>
        /// 判斷兩區間是否相交；第一區間的 null 端點視為無界，包含邊界。
        /// </summary>
        public static bool IsInRange4(DateTime? start, DateTime? end, DateTime start2, DateTime end2)
        {
            return 
                (start == null && end == null) ? true :
                (start != null && end != null) ? (start <= end2 && end >= start2) :
                (start != null && start <= end2) ? true : 
                (end != null && end >= start2);
        }

        /// <summary>
        /// 判斷本機目前時間是否落在含端點的區間；字串多載先取日期，不延伸結束日。
        /// </summary>
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
        /// <summary>
        /// 判斷本機目前時間是否落在含端點的區間；字串多載先取日期，不延伸結束日。
        /// </summary>
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
        /// 取得 UTC 自 1970 年起的秒數並轉成 Int32，受整數範圍限制。
        /// </summary>
        public static int NowSec()
        {
            var ts = DateTime.UtcNow - new DateTime(1970, 1, 1);
            return (int)ts.TotalSeconds;
        }

        /// <summary>
        /// 取得本機時間的 yyyyMMdd_HHmmss 字串。
        /// </summary>
        public static string NowSecStr()
        {
            return DateTime.Now.ToString("yyyyMMdd_HHmmss");
        }

        /// <summary>
        /// 取得本機目前年月 yyyyMM。
        /// </summary>
        public static string NowYm()
        {
            return DateTime.Now.ToString("yyyyMM");
        }
        /// <summary>
        /// 取得本機今日 yyyyMMdd。
        /// </summary>
        public static string NowYmd()
        {
            return DateTime.Now.ToString("yyyyMMdd");
        }

        /// <summary>
        /// 取得本機今日零時。
        /// </summary>
        public static DateTime Today()
        {
            return DateTime.Today;
        }

        /// <summary>
        /// 以共用日期格式輸出本機今日。
        /// </summary>
        public static string TodayCsStr()
        {
            return DateTime.Today.ToString(_Fun.CsDateFmt);
        }

        /// <summary>
        /// 以目前 DbDtFmt 輸出本機現在時間。
        /// </summary>
        public static string NowDbStr()
        {
            return ToDbStr(DateTime.Now);
        }

        /// <summary>
        /// 以共用日期時間格式輸出本機現在時間。
        /// </summary>
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
        /// 依 _Fun.DbDtFmt 格式化時間，不轉換時區。
        /// </summary>
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

        /// <summary>
        /// 依 _Fun.CsDtFmt 格式化時間。
        /// </summary>
        public static string ToCsStr(DateTime dt)
        {
            return dt.ToString(_Fun.CsDtFmt);
        }

        /// <summary>
        /// 依 _Fun.DbDateFmt 格式化日期。
        /// </summary>
        public static string ToDbDateStr(DateTime dt)
        {
            return dt.ToString(_Fun.DbDateFmt);
        }

        /// <summary>
        /// 對 HH:mm 加分鐘，最多校正一次跨時；不處理跨日與大量分鐘進位。
        /// </summary>
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

        /// <summary>
        /// 輸出 HH:mm。
        /// </summary>
        public static string ToHm5(DateTime dt)
        {
            return dt.ToString("HH:mm");
        }

        /// <summary>
        /// 將小時與分鐘各補足兩位，以冒號連接，不驗證範圍。
        /// </summary>
        public static string HmToHm5(int hour, int min)
        {
            return hour.ToString("00") + ":" + min.ToString("00");
        }

        /// <summary>
        /// 依生日計算現有年齡或月齡值；目前年齡公式在生日已到時額外加一，並非實歲。
        /// </summary>
        public static int BirthToAge(DateTime? birth, bool isYear = true)
        {
            if (birth == null) return 0;

            var birth2 = birth.Value;
            var today = DateTime.Today;
            if (isYear)
            {
                //年齡, 有等號
                var age = today.Year - birth2.Year;
                if (birth2.AddYears(age) <= today)
                    age++;  //虛歲
                return age;
            }
            else
            {
                //月齡, 沒有等號
                var months = (today.Year - birth2.Year) * 12 + today.Month - birth2.Month;
                if (birth2.Day < today.Day)
                    months++;
                return months;
            }
        }

        /// <summary>
        /// 轉成自 1970 年起的秒數並固定扣除八小時，不是 .NET ticks。
        /// </summary>
        public static long ToTick(DateTime dt)
        {
            //if (dt == null) return 0;

            var date0 = new DateTime(1970, 1, 1);
            return (dt.Ticks - date0.Ticks) / 10000000 - 8 * 60 * 60;
        }

        /// <summary>
        /// 將自 1970 年起的秒數加到基準時間後呼叫 ToLocalTime。
        /// </summary>
        public static DateTime TickToDt(long tick)
        {
            // Unix timestamp is seconds past epoch
            var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return dt.AddSeconds(tick).ToLocalTime();
        }

        /// <summary>
        /// 解析共用日期時間字串後轉成秒數，空字串回傳 0。
        /// </summary>
        public static long CsToTick(string dts)
        {
            var dt = CsToDt(dts);
            return (dt == null) ? 0 : ToTick(dt.Value);
        }

        /// <summary>
        /// 依 CsDtFmt 解析，短日期補零時；空字串回傳 null，格式失敗實作回傳 DateTime.MinValue。
        /// </summary>
        public static DateTime? CsToDt(string dts)
        {
            if (dts == "") return null;
            if (dts.Length <= 10) dts += " 00:00:00";

            DateTime.TryParseExact(dts, _Fun.CsDtFmt, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt2);
            return dt2;
        }

        /// <summary>
        /// 解析共用日期時間並只保留日期，空字串回傳 null。
        /// </summary>
        public static DateTime? CsToDate(string dts)
        {
            var dt = CsToDt(dts);
            return (dt == null)
                ? null : dt.Value.Date;
        }

        /// <summary>
        /// 輸出共用含秒的日期時間格式。
        /// </summary>
        public static string GetDtStr(DateTime dt)
        {
            return dt.ToString(_Fun.CsDtFmt);
        }

        /// <summary>
        /// 輸出共用不含秒的日期時間格式。
        /// </summary>
        public static string GetDtStr2(DateTime dt)
        {
            return dt.ToString(_Fun.CsDtFmt2);
        }

        /// <summary>
        /// 輸出 yyyy/MM/dd HH:mm:ss-fff，包含毫秒。
        /// </summary>
        public static string GetDtStr3(DateTime dt)
        {
            return dt.ToString("yyyy/MM/dd HH:mm:ss-fff");
        }

        /// <summary>
        /// 取第一個空白前的日期片段，沒有空白時原樣回傳。
        /// </summary>
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
        /// 轉為民國日期；type 1 為中文、2 為斜線、3 為連續數字，其他值回傳 ??。
        /// </summary>
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

        /// <summary>
        /// 嚴格解析 yyyyMMdd；格式不符會拋出例外。
        /// </summary>
        public static DateTime YmdToDate(string ymd)
        {
            return DateTime.ParseExact(ymd, "yyyyMMdd", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 將斜線分隔的西元年月轉為民國年月，type 1、2、3 分別為中文、斜線、數字。
        /// </summary>
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

        /// <summary>
        /// 解析日期字串再轉民國格式；空字串原樣回傳，解析失敗會拋出。
        /// </summary>
        public static string StrToTwDateStr(string dts, int type)
        {
            return (dts == "")
                ? "" : ToTwDateStr(DateTime.Parse(dts), type);
        }

        /// <summary>
        /// 將前三碼民國年加 1911，再解析為西元日期。
        /// </summary>
        public static DateTime TwYmdToDate(string ymd)
        {
            var twYear = int.Parse(ymd[..3]) + 1911;
            return YmdToDate(twYear + ymd[3..]);
        }

        /// <summary>
        /// 將六碼或七碼民國年月日轉成西元斜線日期；其他長度回傳空字串。
        /// </summary>
        public static string TwYmdToDateStr(string ds)
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
        /// <summary>
        /// 取得 0 到 22 的小時選項，目前不包含 23。
        /// </summary>
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
        /// <summary>
        /// 以 step 產生小於 60 的分鐘選項；step 須為正數以避免無限迴圈。
        /// </summary>
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
