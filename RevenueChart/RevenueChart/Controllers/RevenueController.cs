using Newtonsoft.Json.Linq;
using RevenueChart.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RevenueChart.Controllers
{
    public class RevenueController : Controller
    {
        // Connection ID for Data (MSSQL Database)
        private static String DataSource = "REPLACED";
        public ActionResult Index()
        {
            return View();
        }

        /* 
         * ReceiveClickAllow() 
         * Receive/Click/Allow events (‘notify_received’ [event_name] in the Redshift impressions table or [receives] in MSSQL UserMetrics_Rollup table) by 15 minute time-slice, for today, yesterday, and the same day prior week
         * 
         * GeoAdAge()
         * RPM by Core Geo ( ‘US’ , ‘GB’ , ‘CA’ , ‘IN’ , ‘Other’ ), Advertiser (revenuestream), and User Age bucket ( 0-7 days, 8-21 Days, 22+ days )
         * 
         * TotalRev()
         * Total Revenue by Advertiser by day for the trailing 30 (or 60) days
         * 
         * PastRCA()
         * Total Receive/Click/Allow events (‘notify_received’ [event_name] in the Redshift impressions table or [receives] in MSSQL UserMetrics_Rollup table)
         * by day for the trailing 30 (or 60) days
         * along with CTR (click to receive ratio)
         * 
         * TotalPastARCR()
         * Total Allows/Receives/Clicks/Revenue for Today, Yesterday, Last week
         */

        public ContentResult ReceiveClickAllow()
        {
            // Allows / Receives / Clicks events(‘notify_received’ [event_name] in the Redshift impressions table or[receives] in MSSQL UserMetrics_Rollup table)
            // by 15 minute time-slice, for today, yesterday, and the same day prior week
            string Query = @"SELECT
	                            CAST(DATEADD(HOUR,-4,timeslice)AS time) AS timeslice,
	                            CASE CAST(DATEADD(HOUR,-4,timeslice) AS DATE)
		                            WHEN CAST(GETDATE() AS DATE) THEN 'Today'
		                            WHEN CAST(DATEADD(DAY,-1,GETDATE()) AS DATE) THEN 'Yesterday'
		                            WHEN CAST(DATEADD(DAY,-7,GETDATE()) AS DATE) THEN 'Last Week'
	                            END AS DayOfWeek,
	                            SUM(allows) AS allows,
	                            SUM(receives) AS receives, 
	                            SUM(clicks) AS clicks
                            FROM
	                            Reports.dbo.UserMetrics_Rollup WITH(NOLOCK)
                            WHERE 1=1
	                            AND CAST(DATEADD(HOUR,-4,timeslice) AS DATE) IN ( CAST(GETDATE() AS DATE) , CAST(DATEADD(DAY,-1,GETDATE()) AS DATE) , CAST(DATEADD(DAY,-7,GETDATE()) AS DATE) )
                            GROUP BY
	                            DATEADD(HOUR,-4,timeslice),
	                            CASE CAST(DATEADD(HOUR,-4,timeslice) AS DATE)
		                            WHEN CAST(GETDATE() AS DATE) THEN 'Today'
		                            WHEN CAST(DATEADD(DAY,-1,GETDATE()) AS DATE) THEN 'Yesterday'
		                            WHEN CAST(DATEADD(DAY,-7,GETDATE()) AS DATE) THEN 'Last Week'
	                            END
                            ORDER BY
	                            timeslice, DayOfWeek";
            // Since table format is (timeslice, DayOfWeek, Allows, Receives, Clicks), we'll model our Json Array likewise
            // Kinda like a dictionary
            JArray Data = new JArray();
            JArray DateArray = new JArray();
            JArray DayOfWeek = new JArray();
            JArray Allows = new JArray();
            JArray Receives = new JArray();
            JArray Clicks = new JArray();
            JObject Record = new JObject();
            // Connect to MSSQL database using Spigot server
            using (SqlConnection conn = new SqlConnection(DataSource))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(Query, conn);
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();
                // Execute above SQL Query
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader != null && reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            DateArray.Add(reader["timeslice"]);
                            DayOfWeek.Add(reader["DayOfWeek"]);
                            Allows.Add(Convert.ToInt32(reader["allows"]));
                            Receives.Add(Convert.ToInt32(reader["receives"]));
                            Clicks.Add(Convert.ToInt32(reader["clicks"]));

                            Record = new JObject();
                            Record.Add("Date", DateArray);
                            Record.Add("DayOfWeek", DayOfWeek);
                            Record.Add("Allows", Allows);
                            Record.Add("Receives", Receives);
                            Record.Add("Clicks", Clicks);
                        }
                    }
                    reader.Close();
                }
                conn.Close();
            }
            return new ContentResult { Content = Record.ToString(), ContentType = "application/json" };
        }
        public ContentResult ReceiveClickAllowMobile()
        {
            // Allows / Receives / Clicks events(‘notify_received’ [event_name] in the Redshift impressions table or[receives] in MSSQL UserMetrics_Rollup table)
            // by 15 minute time-slice, for today, yesterday, and the same day prior week
            string Query = @"SELECT
	                            CAST(DATEADD(HOUR,-4,timeslice)AS time) AS timeslice,
	                            CASE CAST(DATEADD(HOUR,-4,timeslice) AS DATE)
		                            WHEN CAST(GETDATE() AS DATE) THEN 'Today'
		                            WHEN CAST(DATEADD(DAY,-1,GETDATE()) AS DATE) THEN 'Yesterday'
		                            WHEN CAST(DATEADD(DAY,-7,GETDATE()) AS DATE) THEN 'Last Week'
	                            END AS DayOfWeek,
	                            SUM(allows) AS allows,
	                            SUM(receives) AS receives, 
	                            SUM(clicks) AS clicks
                            FROM
	                            Reports.dbo.UserMetrics_Rollup WITH(NOLOCK)
                            WHERE 1=1
                                AND platform = 'Mobile'
	                            AND CAST(DATEADD(HOUR,-4,timeslice) AS DATE) IN ( CAST(GETDATE() AS DATE) , CAST(DATEADD(DAY,-1,GETDATE()) AS DATE) , CAST(DATEADD(DAY,-7,GETDATE()) AS DATE) )
                            GROUP BY
	                            DATEADD(HOUR,-4,timeslice),
	                            CASE CAST(DATEADD(HOUR,-4,timeslice) AS DATE)
		                            WHEN CAST(GETDATE() AS DATE) THEN 'Today'
		                            WHEN CAST(DATEADD(DAY,-1,GETDATE()) AS DATE) THEN 'Yesterday'
		                            WHEN CAST(DATEADD(DAY,-7,GETDATE()) AS DATE) THEN 'Last Week'
	                            END
                            ORDER BY
	                            timeslice, DayOfWeek";
            // Since table format is (timeslice, DayOfWeek, Allows, Receives, Clicks), we'll model our Json Array likewise
            // Kinda like a dictionary
            JArray Data = new JArray();
            JArray DateArray = new JArray();
            JArray DayOfWeek = new JArray();
            JArray Allows = new JArray();
            JArray Receives = new JArray();
            JArray Clicks = new JArray();
            JObject Record = new JObject();
            // Connect to MSSQL database using Spigot server
            using (SqlConnection conn = new SqlConnection(DataSource))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(Query, conn);
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();
                // Execute above SQL Query
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader != null && reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            DateArray.Add(reader["timeslice"]);
                            DayOfWeek.Add(reader["DayOfWeek"]);
                            Allows.Add(Convert.ToInt32(reader["allows"]));
                            Receives.Add(Convert.ToInt32(reader["receives"]));
                            Clicks.Add(Convert.ToInt32(reader["clicks"]));

                            Record = new JObject();
                            Record.Add("Date", DateArray);
                            Record.Add("DayOfWeek", DayOfWeek);
                            Record.Add("Allows", Allows);
                            Record.Add("Receives", Receives);
                            Record.Add("Clicks", Clicks);
                        }
                    }
                    reader.Close();
                }
                conn.Close();
            }
            return new ContentResult { Content = Record.ToString(), ContentType = "application/json" };
        }
        public ContentResult ReceiveClickAllowDesktop()
        {
            // Allows / Receives / Clicks events(‘notify_received’ [event_name] in the Redshift impressions table or[receives] in MSSQL UserMetrics_Rollup table)
            // by 15 minute time-slice, for today, yesterday, and the same day prior week
            string Query = @"SELECT
	                            CAST(DATEADD(HOUR,-4,timeslice)AS time) AS timeslice,
	                            CASE CAST(DATEADD(HOUR,-4,timeslice) AS DATE)
		                            WHEN CAST(GETDATE() AS DATE) THEN 'Today'
		                            WHEN CAST(DATEADD(DAY,-1,GETDATE()) AS DATE) THEN 'Yesterday'
		                            WHEN CAST(DATEADD(DAY,-7,GETDATE()) AS DATE) THEN 'Last Week'
	                            END AS DayOfWeek,
	                            SUM(allows) AS allows,
	                            SUM(receives) AS receives, 
	                            SUM(clicks) AS clicks
                            FROM
	                            Reports.dbo.UserMetrics_Rollup WITH(NOLOCK)
                            WHERE 1=1
                                AND platform = 'Desktop'
	                            AND CAST(DATEADD(HOUR,-4,timeslice) AS DATE) IN ( CAST(GETDATE() AS DATE) , CAST(DATEADD(DAY,-1,GETDATE()) AS DATE) , CAST(DATEADD(DAY,-7,GETDATE()) AS DATE) )
                            GROUP BY
	                            DATEADD(HOUR,-4,timeslice),
	                            CASE CAST(DATEADD(HOUR,-4,timeslice) AS DATE)
		                            WHEN CAST(GETDATE() AS DATE) THEN 'Today'
		                            WHEN CAST(DATEADD(DAY,-1,GETDATE()) AS DATE) THEN 'Yesterday'
		                            WHEN CAST(DATEADD(DAY,-7,GETDATE()) AS DATE) THEN 'Last Week'
	                            END
                            ORDER BY
	                            timeslice, DayOfWeek";
            // Since table format is (timeslice, DayOfWeek, Allows, Receives, Clicks), we'll model our Json Array likewise
            // Kinda like a dictionary
            JArray Data = new JArray();
            JArray DateArray = new JArray();
            JArray DayOfWeek = new JArray();
            JArray Allows = new JArray();
            JArray Receives = new JArray();
            JArray Clicks = new JArray();
            JObject Record = new JObject();
            // Connect to MSSQL database using Spigot server
            using (SqlConnection conn = new SqlConnection(DataSource))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(Query, conn);
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();
                // Execute above SQL Query
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader != null && reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            DateArray.Add(reader["timeslice"]);
                            DayOfWeek.Add(reader["DayOfWeek"]);
                            Allows.Add(Convert.ToInt32(reader["allows"]));
                            Receives.Add(Convert.ToInt32(reader["receives"]));
                            Clicks.Add(Convert.ToInt32(reader["clicks"]));

                            Record = new JObject();
                            Record.Add("Date", DateArray);
                            Record.Add("DayOfWeek", DayOfWeek);
                            Record.Add("Allows", Allows);
                            Record.Add("Receives", Receives);
                            Record.Add("Clicks", Clicks);
                        }
                    }
                    reader.Close();
                }
                conn.Close();
            }
            return new ContentResult { Content = Record.ToString(), ContentType = "application/json" };
        }
        /* RPM by Core Geo ( ‘US’ , ‘GB’ , ‘CA’ , ‘IN’ , ‘Other’ ), Advertiser (revenuestream), and User Age bucket ( 0-7 days, 8-21 Days, 22+ days )
         * Returns table with country, revenuestream, agebucket, rpm
         */
        public ContentResult GeoAdAge()
        {
            string Query = @"SELECT
                                [date] AS Date
                                , CASE WHEN country IN ( 'US' , 'CA' , 'GB' , 'IN' ) THEN country ELSE 'Other' END AS Country
                                , advertiser AS Advertiser
                                , COALESCE(agebucket,'22+') AS AgeBucket
                                , SUM(revenue) AS Revenue
                                , SUM(clicks) AS Clicks
                                , SUM(receives) AS Receives
                                , COALESCE(1000 * SUM(revenue) / NULLIF(CAST(SUM(receives) AS FLOAT),0),0) AS RPM
                            FROM (
                                SELECT
                                    [date]
                                    , country
                                    , CASE WHEN OfferType = 'indirect' THEN '5th Ave News' ELSE revenuestream END AS Advertiser
                                    , CASE revenuestream
                                        WHEN 'RevContent' THEN
                                            CASE widget_id
                                                WHEN '96581' THEN '0-7'
                                                WHEN '96582' THEN '8-21'
                                                WHEN '96583' THEN '22+'
                                                ELSE '22+'
                                            END
                                        WHEN 'MGID' THEN
                                            CASE widget_id
                                                WHEN 'mgid234431' THEN '0-7'
                                                WHEN 'mgid234432' THEN '8-21'
                                                WHEN 'mgid234434' THEN '22+'
                                                ELSE '22+'
                                            END
                                        WHEN 'ContentAd' THEN
                                            CASE widget_id
                                                WHEN 'ca_514344' THEN '0-7'
                                                WHEN 'ca_514345' THEN '8-21'
                                                WHEN 'ca_514346' THEN '22+'
                                                ELSE '22+'
                                            END
                                    END AS AgeBucket
                                    , SUM(revenue) AS revenue
                                    , 0 AS receives
                                    , 0 AS clicks
                                FROM
                                    Reports.dbo.DailyRevenue
                                WHERE 1=1
                                    AND [date] BETWEEN CAST(DATEADD(DAY,-7,GETDATE()) AS DATE) AND CAST(DATEADD(DAY,-1,GETDATE()) AS DATE)
                                    AND category = 'push'
                                    AND CASE WHEN OfferType = 'indirect' THEN '5th Ave News' ELSE revenuestream END IN ( 'MGID' , 'ContentAd' , 'RevContent', '5th ave news' )
                                GROUP BY
                                    [date]
                                    , country
                                    , CASE WHEN OfferType = 'indirect' THEN '5th Ave News' ELSE revenuestream END
                                    , CASE revenuestream
                                        WHEN 'RevContent' THEN
                                            CASE widget_id
                                                WHEN '96581' THEN '0-7'
                                                WHEN '96582' THEN '8-21'
                                                WHEN '96583' THEN '22+'
                                                ELSE '22+'
                                            END
                                        WHEN 'MGID' THEN
                                            CASE widget_id
                                                WHEN 'mgid234431' THEN '0-7'
                                                WHEN 'mgid234432' THEN '8-21'
                                                WHEN 'mgid234434' THEN '22+'
                                                ELSE '22+'
                                            END
                                        WHEN 'ContentAd' THEN
                                            CASE widget_id
                                                WHEN 'ca_514344' THEN '0-7'
                                                WHEN 'ca_514345' THEN '8-21'
                                                WHEN 'ca_514346' THEN '22+'
                                                ELSE '22+'
                                            END
                                    END
                                UNION
                                SELECT
                                    [date] AS date
                                    , country
                                    , Advertiser
                                    , CASE
                                        WHEN DATEDIFF(DAY,userclass,[date]) < 7 THEN '0-7'
                                        WHEN DATEDIFF(DAY,userclass,[date]) < 21 THEN '8-21'
                                        ELSE '22+'
                                    END AS AgeBucket
                                    , 0 AS revenue
                                    , SUM(receives) AS receives
                                    , SUM(clicks) AS clicks
                                FROM
                                    Reports.dbo.UserMetrics_Rollup
                                WHERE 1=1
                                    AND date BETWEEN CAST(DATEADD(DAY,-7,GETDATE()) AS DATE) AND CAST(DATEADD(DAY,-1,GETDATE()) AS DATE)
                                GROUP BY
                                    date
                                    , country
                                    , Advertiser
                                    , CASE
                                        WHEN DATEDIFF(DAY,userclass,[date]) < 7 THEN '0-7'
                                        WHEN DATEDIFF(DAY,userclass,[date]) < 21 THEN '8-21'
                                        ELSE '22+'
                                    END
                            ) rpm_table
                            WHERE 1=1
                                 AND Advertiser IN ( 'MGID' , 'ContentAd' , 'RevContent', '5th ave news' )
                            GROUP BY
                                [date]
                                , CASE WHEN country IN ( 'US' , 'CA' , 'GB' , 'IN' ) THEN country ELSE 'Other' END
	                            , COALESCE(AgeBucket,'22+')
                                , Advertiser
                                , AgeBucket
                            ORDER BY
                                [date]
                                 , CASE WHEN country IN ( 'US' , 'CA' , 'GB' , 'IN' ) THEN country ELSE 'Other' END
                                , 1000 * SUM(revenue) / NULLIF(CAST(SUM(receives) AS FLOAT),0) DESC
                            ";
            /* Since table format is (country, revenuestream (advertiser), agebucket, rpm), we'll model our Json Array likewise
             * Kinda like a dictionary
             */
            JArray Data = new JArray();
            JArray Country = new JArray();
            JArray Date = new JArray();
            JArray Advertiser = new JArray();
            JArray AgeBucket = new JArray();
            JArray ReceivePerThousand = new JArray();
            JObject Record = new JObject();
            // Connect to MSSQL database using Spigot server
            using (SqlConnection conn = new SqlConnection(DataSource))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(Query, conn);
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();
                // Execute above SQL Query
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader != null && reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Country.Add(reader["Country"]);
                            Advertiser.Add(reader["Advertiser"]);
                            AgeBucket.Add(reader["AgeBucket"]);
                            ReceivePerThousand.Add(Convert.ToDouble(reader["RPM"]));
                            Date.Add(Convert.ToDateTime(reader["Date"]).ToString("MM-dd-yyyy"));

                            Record = new JObject();
                            Record.Add("Country", Country);
                            Record.Add("Advertiser", Advertiser);
                            Record.Add("AgeBucket", AgeBucket);
                            Record.Add("RPM", ReceivePerThousand);
                            Record.Add("Date", Date);
                        }
                    }
                    reader.Close();
                }
                conn.Close();
            }
            return new ContentResult { Content = Record.ToString(), ContentType = "application/json" };
        }

        // Total Revenue by Advertiser by day for the trailing 30 (or 60) days
        public ContentResult TotalRev()
        {
            //Today
            var todaystart = DateTime.UtcNow.AddHours(-24).AddMinutes(-DateTime.UtcNow.Minute).AddSeconds(-DateTime.UtcNow.Second);
            var todayend = DateTime.UtcNow.AddHours(-1).AddMinutes(-DateTime.UtcNow.Minute).AddSeconds(-DateTime.UtcNow.Second);
            //Yesterday
            var yeststart = DateTime.UtcNow.AddHours(-47).AddMinutes(-DateTime.UtcNow.Minute).AddSeconds(-DateTime.UtcNow.Second);
            var yestend = DateTime.UtcNow.AddHours(-24).AddMinutes(-DateTime.UtcNow.Minute).AddSeconds(-DateTime.UtcNow.Second);
            var y = yeststart - yestend;
            //Last Week
            var weekstart = DateTime.UtcNow.AddHours(-24).AddDays(-7).AddMinutes(-DateTime.UtcNow.Minute).AddSeconds(-DateTime.UtcNow.Second);
            var weekend = DateTime.UtcNow.AddHours(-1).AddDays(-7).AddMinutes(-DateTime.UtcNow.Minute).AddSeconds(-DateTime.UtcNow.Second);

            var startOfDay = DateTime.UtcNow.Date.AddDays(-7);
            var endOfDay = DateTime.UtcNow.Date.AddDays(0);

            int[] timearray = new int[96];
            int[,] finaltimearray = new int[96, 60];
            string[] daySlice = new string[60];
            int counter = daySlice.Length;
            for (int i = 0; i < daySlice.Length; i++)
            {
                var day = DateTime.UtcNow.AddDays(-(counter - i));
                daySlice[i] = Convert.ToDateTime(day).ToString("yyyy-MM-dd");
            }
            var monthAgo = DateTime.UtcNow.Date.AddDays(-61);

            DateTime startDate = monthAgo;
            DateTime endDate = endOfDay;
            string[] timeslice = { "00:00", "00:15", "00:30", "00:45", "01:00", "01:15", "01:30", "01:45",
                                    "02:00", "02:15", "02:30", "02:45", "03:00", "03:15", "03:30", "03:45",
                                    "04:00", "04:15", "04:30", "04:45", "05:00", "05:15", "05:30", "05:45",
                                    "06:00", "06:15", "06:30", "06:45", "07:00", "07:15", "07:30", "07:45",
                                    "08:00", "08:15", "08:30", "08:45", "09:00", "09:15", "09:30", "09:45",
                                    "10:00", "10:15", "10:30", "10:45", "11:00", "11:15", "11:30", "11:45",
                                    "12:00", "12:15", "12:30", "12:45", "13:00", "13:15", "13:30", "13:45",
                                    "14:00", "14:15", "14:30", "14:45", "15:00", "15:15", "15:30", "15:45",
                                    "16:00", "16:15", "16:30", "16:45", "17:00", "17:15", "17:30", "17:45",
                                    "18:00", "18:15", "18:30", "18:45", "19:00", "19:15", "19:30", "19:45",
                                    "20:00", "20:15", "20:30", "20:45", "21:00", "21:15", "21:30", "21:45",
                                    "22:00", "22:15", "22:30", "22:45", "23:00", "23:15", "23:30", "23:45", };

            string Query = @"SELECT 
	                            date as 'date',
	                            revenuestream as revenuestream,
	                            SUM(revenue) AS revenue
		                            FROM [Reports].[dbo].[DailyRevenue]
		                            WHERE 1=1 AND date >= dateAdd(DAY, -60, GETDATE()) AND date <= GETDATE() AND category = 'push'
                            group by
	                            revenuestream,
	                            date
		                            order by revenuestream, date";
            // Since table format is (date, revenuestream, revenue), we'll model our Json Array likewise
            // Kinda like a dictionary
            JArray Date = new JArray();
            JArray Data = new JArray();
            JArray SingleDay = new JArray();
            JArray Revenue = new JArray();
            JArray DayTime = new JArray();
            JObject Record = new JObject();
            JObject Day = new JObject();
            using (var sqlConn = new SqlConnection(DataSource))
            {
                sqlConn.Open();
                SqlCommand command = new SqlCommand();
                command.CommandText = string.Format(Query, startDate.ToString(), endDate.ToShortDateString());
                command.CommandType = CommandType.Text;
                command.Connection = sqlConn;
                command.CommandTimeout = 60000;

                string revenuestream = "tt";
                using (var reader = command.ExecuteReader())
                {
                    if (reader != null && reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            string temp = Convert.ToDateTime(reader["date"]).ToString("yyyy-MM-dd");
                            if (revenuestream != "tt" && revenuestream != Convert.ToString(reader["revenuestream"]))
                            {
                                while (DayTime.Count < daySlice.Length)
                                {
                                    DayTime.Add(daySlice[DayTime.Count]);
                                    Revenue.Add(0);
                                }
                                Record = new JObject();
                                Record.Add("Advertiser", revenuestream);
                                Record.Add("Revenue", Revenue);
                                Record.Add("Date", DayTime);

                                Data.Add(Record);

                                DayTime = new JArray();
                                Revenue = new JArray();
                            }
                            revenuestream = Convert.ToString(reader["revenuestream"]);
                            if (daySlice.Length != DayTime.Count)
                            {
                                while (daySlice[DayTime.Count] != temp)
                                {
                                    DayTime.Add(daySlice[DayTime.Count]);
                                    Revenue.Add(0);
                                }
                            }
                            DayTime.Add(Convert.ToDateTime(reader["date"]).ToString("yyyy-MM-dd"));
                            Revenue.Add(Convert.ToInt32(reader["revenue"] is DBNull ? 0 : reader["revenue"]));
                        }
                    }
                }
                Record = new JObject();
                while (DayTime.Count < daySlice.Length)
                {
                    DayTime.Add(daySlice[DayTime.Count]);
                    Revenue.Add(0);              
                }
                Record.Add("Advertiser", revenuestream);
                Record.Add("Revenue", Revenue);
                Record.Add("Date", DayTime);
                Data.Add(Record);
                sqlConn.Close();
            }
            return new ContentResult { Content = Data.ToString(), ContentType = "application/json" };
        }

        /* PastRCA()
         * Total Receive/Click/Allow events(‘notify_received’ [event_name] in the Redshift impressions table or[receives] in MSSQL UserMetrics_Rollup table)
         * by day for the trailing 30 (or 60) days
         * along with CTR(click to receive ratio)
         */
        public ContentResult PastRCA()
        {
            string Query = @"DECLARE @TimeDiff INT;
                            SELECT @TimeDiff = DateDiff(HOUR, Reports.dbo.get_et(), getDate())
                            SELECT
                                CAST(DATEADD(HOUR, -@TimeDiff, timeslice) AS DATE) as date,
	                            SUM(receives) as receives,
	                            SUM(clicks) as clicks,
	                            SUM(allows) as allows,
	                            100.0 * SUM(clicks) / SUM(receives) as ctr

                                    FROM[Reports].[dbo].[UserMetrics_Rollup]

                                    WHERE 1 = 1 AND date >= dateAdd(DAY, -30, GETDATE()) AND date <= GETDATE()
                            GROUP BY
                               CAST(DATEADD(HOUR, -@TimeDiff, timeslice) AS DATE)
                            ORDER BY 
                               CAST(DATEADD(HOUR, -@TimeDiff, timeslice) AS DATE)";
            // Since table format is (date, receives, clicks, allows, ctr), we'll model our Json Array likewise
            // Kinda like a dictionary
            JArray Data = new JArray();
            JArray Date = new JArray();
            JArray Receives = new JArray();
            JArray Clicks = new JArray();
            JArray Allows = new JArray();
            JArray ClickThroughRate = new JArray();
            JObject Record = new JObject();
            // Connect to MSSQL database using Spigot server
            using (SqlConnection conn = new SqlConnection(DataSource))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(Query, conn);
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();
                // Execute above SQL Query
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader != null && reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Date.Add(reader["date"]);
                            Receives.Add(Convert.ToInt32(reader["receives"]));
                            Clicks.Add(Convert.ToInt32(reader["clicks"]));
                            Allows.Add(Convert.ToInt32(reader["allows"]));
                            ClickThroughRate.Add(Convert.ToDouble(reader["ctr"]));

                            Record = new JObject();
                            Record.Add("Date", Date);
                            Record.Add("Receives", Receives);
                            Record.Add("Clicks", Clicks);
                            Record.Add("Allows", Allows);
                            Record.Add("CTR", ClickThroughRate);
                        }
                    }
                    reader.Close();
                }
                conn.Close();
            }
            return new ContentResult { Content = Record.ToString(), ContentType = "application/json" };
        }

        /*
         * Total Allows/Receives/Clicks/Revenue for Today, Yesterday, Last week
         */
        public ContentResult TotalPastARCR()
        {
            string Query = @"SELECT
								usermetrics.date,
	                            usermetrics.DayOfWeek,
								FORMAT(usermetrics.receives, '##,##0') as receives,
	                            FORMAT(usermetrics.clicks, '##,##0') as clicks,
	                            FORMAT(usermetrics.allows, '##,##0') as allows,
	                            CAST(100.0 * usermetrics.clicks/usermetrics.receives AS DECIMAL(16,1)) as ctr,
	                            PARSENAME(CONVERT(VARCHAR,CAST(revenuedata.revenue AS MONEY),1),2) as revenue,
	                            CAST(revenuedata.revenue/usermetrics.clicks AS DECIMAL(16,2)) as rpc,
	                            CAST(1000 * revenuedata.revenue/usermetrics.receives AS DECIMAL(16,2)) as rpm,
								FORMAT(usermetrics.totime_clicks, '##,##0') AS totime_clicks
                            FROM (
	                            SELECT
		                            CAST(DATEADD(HOUR, -4, timeslice) AS DATE) AS [date],
		                            CASE CAST(DATEADD(HOUR, -4, timeslice) AS DATE)
			                            WHEN CAST(GETDATE() AS DATE) THEN 'Today'
			                            WHEN CAST(DATEADD(DAY,-1,GETDATE()) AS DATE) THEN 'Yesterday'
			                            WHEN CAST(DATEADD(DAY,-7,GETDATE()) AS DATE) THEN 'Last Week'
		                            END AS DayOfWeek,
									SUM(CASE WHEN CAST(timeslice AS TIME) < CAST(GETDATE() AS TIME) THEN clicks ELSE 0 END) AS totime_clicks,
		                            SUM(allows) AS allows,
		                            SUM(receives) AS receives,
		                            SUM(clicks) AS clicks
	                            FROM Reports.dbo.UserMetrics_Rollup WITH(NOLOCK)
	                            WHERE 1=1
		                            AND CAST(DATEADD(HOUR, -4, timeslice) AS DATE) IN (CAST(GETDATE() AS DATE) , CAST(DATEADD(DAY,-1,GETDATE()) AS DATE) , CAST(DATEADD(DAY,-7,GETDATE()) AS DATE) )
	                            GROUP BY
		                            CAST(DATEADD(HOUR, -4, timeslice) AS DATE),
		                            CASE CAST(DATEADD(HOUR, -4, timeslice) AS DATE)
			                            WHEN CAST(GETDATE() AS DATE) THEN 'Today'
			                            WHEN CAST(DATEADD(DAY,-1,GETDATE()) AS DATE) THEN 'Yesterday'
			                            WHEN CAST(DATEADD(DAY,-7,GETDATE()) AS DATE) THEN 'Last Week'
		                            END
	                            ) AS usermetrics
	                            -- Seperate 2 tables and join them
	                            LEFT JOIN (
	                            SELECT
		                            CAST(DATEADD(DAY, 0, date) AS DATE) AS [date],
		                            CASE CAST(DATEADD(DAY, 0, date) AS DATE)
			                            WHEN CAST(GETDATE() AS DATE) THEN 'Today'
			                            WHEN CAST(DATEADD(DAY,-1,GETDATE()) AS DATE) THEN 'Yesterday'
			                            WHEN CAST(DATEADD(DAY,-7,GETDATE()) AS DATE) THEN 'Last Week'
		                            END AS DayOfWeek,
		                            SUM(revenue) as revenue
	                            FROM Reports.dbo.DailyRevenue
	                            WHERE 1=1
		                            AND date IN (CAST(GETDATE() AS DATE), CAST(DATEADD(DAY,-1,GETDATE()) AS DATE), CAST(DATEADD(DAY,-7,GETDATE()) AS DATE))
		                            AND category = 'push'
	                            GROUP BY
		                            date,
		                            CAST(DATEADD(DAY, 0, date) AS DATE),
		                            CASE CAST(DATEADD(DAY, 0, date) AS DATE)
			                            WHEN CAST(GETDATE() AS DATE) THEN 'Today'
			                            WHEN CAST(DATEADD(DAY,-1,GETDATE()) AS DATE) THEN 'Yesterday'
			                            WHEN CAST(DATEADD(DAY,-7,GETDATE()) AS DATE) THEN 'Last Week'
		                            END
	                            ) as revenuedata ON revenuedata.date = usermetrics.date
                            ORDER BY
	                            usermetrics.date DESC";
            // Since table format is (date, DayOfWeek, receives, clicks, allows, ctr, revenue, rpc, rpm), we'll model our Json Array likewise
            // Kinda like a dictionary
            var PredRev = 0.0;
            var ClicksYThisTime = 0.0;
            var ClicksY = 0.0;
            var ClicksT = 0.0;
            var RPCLW = 0.0;
            var RPCY = 0.0;
            // Connect to MSSQL database using Spigot server
            using (SqlConnection conn = new SqlConnection(DataSource))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(Query, conn);
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();
                // Execute above SQL Query
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader != null && reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (reader["DayOfWeek"].ToString() == "Today")
                            {
                                ClicksT = Convert.ToDouble(reader["clicks"].ToString().Replace(",",""));
                            }
                            if (reader["DayOfWeek"].ToString() == "Yesterday")
                            {
                                ClicksYThisTime = Convert.ToDouble(reader["totime_clicks"].ToString().Replace(",", ""));
                                ClicksY = Convert.ToDouble(reader["clicks"].ToString().Replace(",", ""));
                                RPCY = Convert.ToDouble(reader["rpc"]);
                            }
                            if (reader["DayOfWeek"].ToString() == "Last Week")
                            {
                                RPCLW = Convert.ToDouble(reader["rpc"]);
                            }
                        }
                    }
                    reader.Close();
                }
                conn.Close();
            }
            PredRev = ClicksY * (ClicksT / ClicksYThisTime * 1.0) * (RPCLW + RPCY) / 2;
            return new ContentResult { Content = String.Format("{0:0,0}", PredRev), ContentType = "text/plain" };
        }
    }
}