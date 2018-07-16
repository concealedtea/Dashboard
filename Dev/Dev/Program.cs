using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Dev
{
    class Program
    {
        private static String DataSource = "Data Source=push.ctwzprc2znex.us-east-1.rds.amazonaws.com; Initial Catalog=Reports;User Id=bob;Password=industrylawpricesomewhere;";
        static void Main(string[] args)
        {
            /* 
        * Receive events (‘notify_received’ [event_name] in the Redshift impressions table or [receives] in MSSQL UserMetrics_Rollup table) by 15 minute time-slice, for today, yesterday, and the same day prior week
        * Click events (‘notify_click’ [event_name] in the Redshift impressions table or [clicks] in MSSQL UserMetrics_Rollup table) by 15 minute time-slice, for today, yesterday, and the same day prior week
        * Allow events (‘notify_allow’ [event_name] in the Redshift impressions table or [allows] in MSSQL UserMetrics_Rollup table) by 15 minute time-slice, for today, yesterday, and the same day prior week
        */
                // Allows / Receives / Clicks events(‘notify_received’ [event_name] in the Redshift impressions table or[receives] in MSSQL UserMetrics_Rollup table)
                // by 15 minute time-slice, for today, yesterday, and the same day prior week
                string Query = @"SELECT
	                            DATEADD(HOUR,-4,timeslice) AS timeslice,
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
	                            DATEADD(HOUR,-4,timeslice) DESC";
                // Since table format is (timeslice, DayOfWeek, Allows, Receives, Clicks), we'll model our Json Array likewise
                JArray Data = new JArray();
                JArray DateArray = new JArray();
                JArray DayOfWeek = new JArray();
                JArray Allows = new JArray();
                JArray Receives = new JArray();
                JArray Clicks = new JArray();
                JObject Record = new JObject();
                using (SqlConnection conn = new SqlConnection(DataSource))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(Query, conn);
                    cmd.CommandTimeout = 0;
                    cmd.ExecuteNonQuery();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader != null && reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                DateArray.Add(Convert.ToDateTime(reader["timeslice"]));
                                DayOfWeek.Add(Convert.ToString(reader["DayOfWeek"]));
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
                    string var = Record.ToString();
                    Console.WriteLine(var);
                    }
                }
            }
    }
}
