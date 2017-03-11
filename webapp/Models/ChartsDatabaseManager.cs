﻿using Highsoft.Web.Mvc.Charts;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;

namespace ChartsMix.Models
{
    public class ChartsDatabaseManager
    {
        public string _connectionString = ConfigurationManager.ConnectionStrings["cmixDbConnection"].ConnectionString;
        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
            set
            {
                _connectionString = value;
            }
        }
        public string _groupConnectionString = ConfigurationManager.ConnectionStrings["groups"].ConnectionString;
        public string groupConnectionString
        {
            get
            {
                return _groupConnectionString;
            }
            set
            {
                _groupConnectionString = value;
            }
        }

        private static T GetValue<T>(object readerValue, T defaultValue = default(T))
        {
            if (readerValue == DBNull.Value)
                return defaultValue;
            else
                return (T)Convert.ChangeType(readerValue, typeof(T));
        }

        public List<PieSeriesData> GetPieChartMeters(int[] ids, DateTime fromDate, DateTime toDate, PiePeriod period)
        {
            var result = new List<PieSeriesData>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand();
                    command.Connection = connection;
                    command.Parameters.Add(new SqlParameter("@Ids", string.Join(",", ids)));
                    switch (period)
                    {
                        case PiePeriod.Day:
                            fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 0, 0);
                            fromDate = fromDate.AddSeconds(-fromDate.Second);
                            toDate = DateTime.Now;
                            break;
                        case PiePeriod.Week:
                            fromDate = DateTime.Now;
                            fromDate = fromDate.AddDays(-7);
                            fromDate = fromDate.AddSeconds(-fromDate.Second);
                            toDate = DateTime.Now;
                            break;
                        case PiePeriod.Month:
                            fromDate = DateTime.Now;
                            fromDate = fromDate.AddMonths(-1);
                            fromDate = fromDate.AddSeconds(-fromDate.Second);
                            toDate = DateTime.Now;
                            break;
                        case PiePeriod.Year:
                            fromDate = DateTime.Now;
                            fromDate = fromDate.AddYears(-1);
                            fromDate = fromDate.AddSeconds(-fromDate.Second);
                            toDate = DateTime.Now;
                            break;
                        case PiePeriod.Custom:
                            fromDate = fromDate.AddSeconds(-fromDate.Second);
                            toDate = toDate.AddSeconds(59 - toDate.Second);
                            break;
                    }
                    command.Parameters.Add(new SqlParameter("@From", fromDate));
                    command.Parameters.Add(new SqlParameter("@To", toDate));

                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.CommandText = "Get_Pie3D_Chart";
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new PieSeriesData
                            {
                                Name = GetValue<string>(reader["Name"], string.Empty),
                                Y = GetValue<double>(reader["FloatVALUE"], 0.0)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        public List<Meter> GetAllMeters()
        {
            var result = new List<Meter>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand("Select * from tbTrendLogRelation", connection);
                    command.Connection = connection;
                    command.CommandType = System.Data.CommandType.Text;
                    command.CommandText = "Select GUID, ParentGuid, EntityID, Name, Description, Path, Type from tbTrendLogRelation";
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new Meter
                            {
                                Id = new Guid(reader["GUID"].ToString()),
                                ParentId = new Guid(reader["ParentGUID"].ToString()),
                                EntityId = GetValue<int>(reader["EntityID"], 0),
                                Name = GetValue<string>(reader["Name"], string.Empty),
                                Description = GetValue<string>(reader["Description"], string.Empty),
                                Path = GetValue<string>(reader["Path"], string.Empty),
                                Type = GetValue<string>(reader["Type"], string.Empty)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        public Meter GetMeterTree()
        {
            return FormTree(GetAllMeters());
        }

        private Meter FormTree(List<Meter> meters)
        {
            var server = new Meter();
            server.Name = "Server";
            server.Type = "system.base.Folder";
            foreach (var meter in meters)
            {
                var node = meters.FirstOrDefault(m => m.Id == meter.ParentId);
                if (node == null)
                {
                    meter.Parent = server;
                    server.Children.Add(meter);
                }
                else
                {
                    meter.Parent = node;
                    node.Children.Add(meter);
                }
            }

            return server;
        }


        // New
        public List<Series> GetBarChartMeters(int[] ids, DateTime fromDate, DateTime toDate, BarPeriod period, out List<string> dates)
        {
            var result = new List<Series>();

            List<double> tokyoValues = new List<double> { 49.9, 71.5, 106.4, 129.2, 144.0, 176.0, 135.6, 148.5, 216.4, 194.1, 95.6, 54.4 };
            List<double> nyValues = new List<double> { 83.6, 78.8, 98.5, 93.4, 106.0, 84.5, 105.0, 104.3, 91.2, 83.5, 106.6, 92.3 };
            List<double> berlinValues = new List<double> { 42.4, 33.2, 34.5, 39.7, 52.6, 75.5, 57.4, 60.4, 47.6, 39.1, 46.8, 51.1 };
            List<double> londonValues = new List<double> { 48.9, 38.8, 39.3, 41.4, 47.0, 48.3, 59.0, 59.6, 52.4, 65.2, 59.3, 51.2 };
            List<ColumnSeriesData> tokyoData = new List<ColumnSeriesData>();
            List<ColumnSeriesData> nyData = new List<ColumnSeriesData>();
            List<ColumnSeriesData> berlinData = new List<ColumnSeriesData>();
            List<ColumnSeriesData> londonData = new List<ColumnSeriesData>();

            tokyoValues.ForEach(p => tokyoData.Add(new ColumnSeriesData { Y = p }));
            nyValues.ForEach(p => nyData.Add(new ColumnSeriesData { Y = p }));
            berlinValues.ForEach(p => berlinData.Add(new ColumnSeriesData { Y = p }));
            londonValues.ForEach(p => londonData.Add(new ColumnSeriesData { Y = p }));
            result = new List<Series>
            {
                new ColumnSeries
                {
                    Name = "Tokyo",
                    Data = tokyoData
                },
                new ColumnSeries
                {
                    Name = "New York",
                    Data = nyData
                },
                new ColumnSeries
                {
                    Name = "Berlin",
                    Data = berlinData
                },
                new ColumnSeries
                {
                    Name = "London",
                    Data = londonData
                }
            };

            dates = new List<string> {
                        "1",
                        "2",
                        "Mar",
                        "Apr",
                        "May",
                        "Jun",
                        "Jul",
                        "Aug",
                        "Sep",
                        "Oct",
                        "Nov",
                        "Dec"
                    };
            return result;
        }

        private List<CustomLineSeries> HandleLineChart(int numberOfReturnedValues, DateTime From, DateTime To, BarPeriod period, params int[] ids)
        {
            try
            {

                var result = new List<CustomLineSeries>();
                foreach (var meterId in ids)
                {
                    var queryResult = new List<LineChartItem>();
                    using (SqlConnection connection = new SqlConnection(_connectionString))
                    {
                        var command = new SqlCommand();
                        command.Connection = connection;
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.CommandText = "Bar_Chart_Meter";

                        command.Parameters.Add(new SqlParameter("@Id", meterId));
                        command.Parameters.Add(new SqlParameter("@From", From));
                        command.Parameters.Add(new SqlParameter("@Period", (int)period));
                        command.Parameters.Add(new SqlParameter("@To", To));

                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                queryResult.Add(new LineChartItem
                                {
                                    Name = GetValue<string>(reader["Name"], string.Empty),
                                    entityId = GetValue<int>(reader["entityID"], 0),
                                    value = GetValue<double>(reader["FloatVALUE"], 0.0),
                                    date = GetValue<DateTime>(reader["DateTimeStamp"], DateTime.MinValue)
                                });

                            }
                        }

                    }
                    var meter = queryResult.FirstOrDefault(m => m.entityId == meterId);
                    if(meter != null)
                    {
                        var dataResult = new List<LineSeriesData>();
                        while (queryResult.Count <= numberOfReturnedValues)
                        {
                            queryResult.Add(new LineChartItem
                            {
                                Name = meter.Name,
                                entityId = meter.entityId,
                                value = 0,
                                date = (meter.date < CleanDateTimeWeek(To)) ? DateTime.MaxValue : DateTime.MinValue
                            });
                        }

                        queryResult = queryResult.OrderByDescending(m => m.date).ToList();
                        for (int i = 0; i < queryResult.Count - 1; i++)
                        {
                            dataResult.Add(new LineSeriesData
                            {
                                Y = queryResult[i].value - queryResult[i + 1].value
                            });
                        }
                        dataResult.Reverse();

                        result.Add(new CustomLineSeries
                        {
                            Name = meter.Name,
                            things = dataResult
                        });
                    }
                    else
                    {
                        //
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // In Progress
        public List<CustomLineSeries> GetLineChartMeters(out ChartDetails details, DateTime fromDate, DateTime toDate, BarPeriod period, int[] ids)
        {
            details = new ChartDetails();
            List<CustomLineSeries> result = new List<CustomLineSeries>();
            switch (period)
            {
                case BarPeriod.Day:
                    fromDate = DateTime.Now.AddHours(-25);
                    fromDate = CleanDateTime(fromDate);
                    toDate = CleanDateTime(DateTime.Now).AddSeconds(-1);
                    details.Title = "By Day Report";
                    details.SubTitle = "Last 24 Hours Report";
                    details.Dates = GenerateHours();
                    result = HandleLineChart(24, fromDate, toDate, period, ids);
                    break;
                case BarPeriod.Week:
                    fromDate = DateTime.Now.AddDays(-8);
                    fromDate = CleanDateTimeWeek(fromDate);
                    toDate = CleanDateTimeWeek(DateTime.Now).AddSeconds(-1);
                    details.Title = "Week Report";
                    details.SubTitle = "Last 7 Days Report";
                    (details.Dates = GenerateDays()).Reverse();
                    result = HandleLineChart(7, fromDate, toDate, period, ids);
                    break;
                case BarPeriod.Year:
                    fromDate = DateTime.Now.AddMonths(-13);
                    fromDate = CleanDateTimeYear(fromDate);
                    toDate = CleanDateTimeYear(DateTime.Now).AddSeconds(-1);
                    details.Title = "Year Report";
                    details.SubTitle = "Last 12 Months Report";
                    (details.Dates = GenerateMonths()).Reverse();
                    result = HandleLineChart(12, fromDate, toDate, period, ids);
                    break;
                case BarPeriod.Custom:
                    details.Dates = null;
                    break;
                default:
                    details.Dates = null;
                    break;
            }
            return result;
        }

        #region private Helpers

        private DateTime CleanDateTime(DateTime dateTime)
        {
            dateTime = dateTime.AddMinutes(-dateTime.Minute);
            dateTime = dateTime.AddSeconds(-dateTime.Second);
            dateTime = dateTime.AddMilliseconds(-dateTime.Millisecond);
            return dateTime;
        }

        private DateTime CleanDateTimeWeek(DateTime dateTime)
        {
            dateTime = dateTime.AddHours(-dateTime.Hour);
            return CleanDateTime(dateTime);
        }

        private DateTime CleanDateTimeYear(DateTime dateTime)
        {
            dateTime = dateTime.AddDays(-dateTime.Day);
            return CleanDateTimeWeek(dateTime);
        }

        private List<string> GenerateMonths()
        {
            var date = DateTime.Now;
            List<string> dates = new List<string>();
            for (int i = 1; i <= 12; i++)
                dates.Add(date.AddMonths(-i).ToString("MMYY"));
            return dates;
        }

        private List<string> GenerateDays()
        {
            var date = DateTime.Now;
            List<string> dates = new List<string>();
            for (int i = 1; i <= 7; i++)
                dates.Add(date.AddDays(-i).ToString("dd"));
            return dates;
        }

        private List<string> GenerateHours()
        {
            var date = DateTime.Now;
            List<string> dates = new List<string>();
            for (int i = 1; i <= 24; i++)
                dates.Add(date.AddHours(-i).ToString("hh"));
            return dates;
        }
        #endregion

        public List<Group> GetAllGroups()
        {
            var result = new List<Group>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_groupConnectionString))
                {
                    var command = new SqlCommand();
                    command.Connection = connection;
                    command.CommandType = System.Data.CommandType.Text;
                    command.CommandText = "Select * from Groups";
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new Group
                            {
                                Id = GetValue<int>(reader["Id"], 0),
                                Name = GetValue<string>(reader["Name"], string.Empty),
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        public int AddGroup(Group group)
        {
            var result = new List<Group>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_groupConnectionString))
                {
                    var command = new SqlCommand();
                    command.Connection = connection;
                    command.CommandType = System.Data.CommandType.Text;
                    command.CommandText = "insert into Groups OUTPUT Inserted.Id values (@Name)";
                    command.Parameters.AddWithValue("@Name", group.Name);
                    connection.Open();
                    int groupId = (int)command.ExecuteScalar();
                    if (group.Ids != null)
                    {
                        foreach (int id in group.Ids)
                        {
                            AssignMeterToGroup(id, groupId);
                        }
                    }

                    return groupId;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void AssignMeterToGroup(int EntityId, int GroupId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_groupConnectionString))
                {
                    var command = new SqlCommand();
                    command.Connection = connection;
                    command.CommandType = System.Data.CommandType.Text;
                    command.CommandText = "insert into Meters values (@EntityId, @GroupId)";
                    command.Parameters.AddWithValue("@EntityId", EntityId);
                    command.Parameters.AddWithValue("@GroupId", GroupId);
                    connection.Open();
                    command.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}