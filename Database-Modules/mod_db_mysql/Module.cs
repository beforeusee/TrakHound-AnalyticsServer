using MySql.Data.MySqlClient;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using TrakHound.Api.v2;
using TrakHound.Api.v2.Data;
using TrakHound.Api.v2.Streams;
using TrakHound.Api.v2.Streams.Data;

namespace mod_db_mysql
{
    [InheritedExport(typeof(IDatabaseModule))]
    class Module:IDatabaseModule
    {
        private Configuration configuration;

        private const string CONNECTION_FORMAT = "server={0};uid={1}; pwd={2}; database={3};default command timeout=300;";

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static string connectionString;

        public string Name
        {
            get
            {
                return "MySql";
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public Module()
        {

        }

        private string EscapeString(string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                return MySqlHelper.EscapeString(s);
            }
            return s;
        }

        /// <summary>
        /// 初始化数据库模块
        /// </summary>
        /// <param name="databaseConfigurationPath"></param>
        /// <returns></returns>
        public bool Initialize(string databaseConfigurationPath)
        {

            Configuration configuration = Configuration.Get(databaseConfigurationPath);

            if (configuration != null)
            {
                Module.connectionString = string.Format("server={0};uid={1};pwd={2};database={3};default command timeout=300;",new object[]
                {
                    configuration.Server,
                    configuration.User,
                    configuration.Password,
                    configuration.Database
                });

                this.configuration = configuration;

                return true;
            }
            return false;
        }

        /// <summary>
        /// 关闭数据库模块
        /// </summary>
        public void Close()
        {

        }

        #region "Read"

        private static T Read<T>(string query)
        {
            if (!string.IsNullOrEmpty(query))
            {
                try
                {
                    //Create a new SqlConnection using the connectionString
                    using (var connection = new MySqlConnection(connectionString))
                    {
                        //Open the connection
                        connection.Open();

                        using (var command = new MySqlCommand(query, connection))
                        {
                            using (MySqlDataReader reader = command.ExecuteReader())
                            {
                                reader.Read();

                                return Read<T>(reader);
                            }
                        }

                    }
                    
                }
                catch (Exception ex)
                {
                    logger.Error("MySql Query Error :" + query);
                    logger.Error(ex);
                }
            }

            return default(T);
        }

        private static List<T> ReadList<T>(string query)
        {
            if (!string.IsNullOrEmpty(query))
            {
                try
                {
                    var list = new List<T>();

                    //Create a new SqlConnection using the connectionString
                    using (var connection = new MySqlConnection(connectionString))
                    {
                        //Open the connection
                        connection.Open();

                        using (var command = new MySqlCommand(query, connection))
                        {
                            using (MySqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    list.Add(Read<T>(reader));
                                }
                            }
                        }

                    }
                    return list;
                }
                catch (Exception ex)
                {
                    logger.Error("MySql Query Error :" + query);
                    logger.Error(ex);
                }
            }
            return null;
        }


        private static T Read<T>(MySqlDataReader reader)
        {
            var obj = (T)Activator.CreateInstance(typeof(T));

            //Get object's properties
            var properties = typeof(T).GetProperties().ToList();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var column = reader.GetName(i);
                var value = reader.GetValue(i);

                var property = properties.Find(o => PropertyToColumn(o.Name) == column);
                if (property != null&&value!=null)
                {
                    object val = default(T);

                    if (property.PropertyType == typeof(string))
                    {
                        string s = value.ToString();
                        if (!string.IsNullOrEmpty(s))
                        {
                            val = s;
                        }
                    }
                    else if (property.PropertyType == typeof(DateTime))
                    {
                        long ms = (long)value;
                        val = UnixTimeExtensions.EpochTime.AddMilliseconds(ms);
                    }
                    else
                    {
                        val = Convert.ChangeType(value, property.PropertyType);
                    }

                    property.SetValue(obj, val, null);
                }
            }

            return obj;
        }

        private static string PropertyToColumn(string propertyName)
        {
            if (propertyName != propertyName.ToUpper())
            {
                // Split string by Uppercase characters
                var parts = Regex.Split(propertyName, @"(?<!^)(?=[A-Z])");
                string s = string.Join("_", parts);
                return s.ToLower();
            }
            else return propertyName.ToLower();
        }

        /// <summary>
        /// read Agent according to deviceId
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public AgentDefinition ReadAgent(string deviceId)
        {
            string str = string.Format("SELECT * FROM `agents` WHERE `device_id`='{0}' ORDER BY `timestamp` DESC LIMIT 1", deviceId);
            
            return Read<AgentDefinition>(str);
        }

        /// <summary>
        /// get the list of Assets，注意：此处的SQL语句可能会出错，请将类似{deviceId}的替换为正确的格式。
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="assetId"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="at"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<AssetDefinition> ReadAssets(string deviceId,string assetId,DateTime from,DateTime to,DateTime at,long count)
        {
            List<AssetDefinition> list = new List<AssetDefinition>();
            string str = "*";
            string str2 = "assets";
            string str3 = "";
            if (!string.IsNullOrEmpty(str3))
            {
                str3 = " AND `id`='" + assetId + "'";
            }
            string str4 = null;
            if ((from > DateTime.MinValue) && (to > DateTime.MinValue))
            {
                str4 = "SELECT {str} FROM `{str2}` WHERE `device_id` = '{deviceId}'{str3} AND `timestamp` >= '{from.ToUnixTime()}' AND `timestamp` <= '{to.ToUnixTime()}'";
            }
            else if ((from > DateTime.MinValue) && (count > 0L))
            {
                str4 = "SELECT {str} FROM `{str2}` WHERE `device_id` = '{deviceId}'{str3} AND `timestamp` >= '{from.ToUnixTime()}' LIMIT {count}";
            }
            else if ((to > DateTime.MinValue) && (count > 0L))
            {
                str4 = "SELECT {str} FROM `{str2}` WHERE `device_id` = '{deviceId}'{str3} AND `timestamp` <= '{to.ToUnixTime()}' LIMIT {count}";
            }
            else if (from > DateTime.MinValue)
            {
                str4 = string.Format("SELECT {0} FROM `{1}` WHERE `device_id` = '{2}'{3} AND `timestamp` >= '{4}' LIMIT 1000", new object[] { str, str2, deviceId, str3, from.ToUnixTime(), count });
            }
            else if (to > DateTime.MinValue)
            {
                str4 = string.Format("SELECT {0} FROM `{1}` WHERE `device_id` = '{2}'{3} AND `timestamp` <= '{4}' LIMIT 1000", new object[] { str, str2, deviceId, str3, to.ToUnixTime(), count });
            }
            else if (count > 0L)
            {
                str4 = "SELECT {str} FROM `{str2}` WHERE `device_id` = '{deviceId}'{str3} ORDER BY `timestamp` DESC LIMIT {count}";
            }
            else if (at > DateTime.MinValue)
            {
                str4 = "SELECT {str} FROM `{str2}` WHERE `device_id` = '{deviceId}'{str3} AND `timestamp` = '{at.ToUnixTime()}'";
            }
            else
            {
                str4 = "SELECT {str} FROM `{str2}` WHERE `device_id` = '{deviceId}'{str3}";
            }
            if (!string.IsNullOrEmpty(str4))
            {
                list = ReadList<AssetDefinition>(str4);
            }
            return list;
        }

        /// <summary>
        /// read the Components of device
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="agentInstanceId"></param>
        /// <returns></returns>
        public List<ComponentDefinition> ReadComponents(string deviceId, long agentInstanceId)
        {
            var str = string.Format("SELECT * FROM `components` WHERE `device_id` = '{0}' AND `agent_instance_id` = {1}", new object[]{this.EscapeString(deviceId), agentInstanceId});
            
            return ReadList<ComponentDefinition>(str);
        }

        public bool DeleteRejectedPart(string deviceId, string partId)
        {
            if (!deviceId.IsNullOrEmpty<char>() && !partId.IsNullOrEmpty<char>())
            {

                string query = string.Format("DELETE FROM `parts_rejected` WHERE {(`device_id`='{0}' AND `part_id`='{1}')}", new object[]{this.EscapeString(deviceId), this.EscapeString(partId)});
                return this.Write(query);
            }
            return false;
        }

        public bool DeleteVerifiedPart(string deviceId, string partId)
        {
            if (!deviceId.IsNullOrEmpty<char>() && !partId.IsNullOrEmpty<char>())
            {
                string query = string.Format("DELETE FROM `parts_verified` WHERE {(`device_id`='{0}' AND `part_id`='{1}')}", new object[]{this.EscapeString(deviceId), this.EscapeString(partId)});
                return this.Write(query);
            }
            return false;
        }

        public ConnectionDefinition ReadConnection(string deviceId)
        {
            var str = string.Format("SELECT * FROM `connections` WHERE `device_id` = '{0}' LIMIT 1", deviceId);
            return Read<ConnectionDefinition>(str);
        }

        public List<ConnectionDefinition> ReadConnections()
        {
            string str="SELECT * FROM `connections`";

            return ReadList<ConnectionDefinition>(str);
        }

        public List<DataItemDefinition> ReadDataItems(string deviceId, long agentInstanceId)
        {
            var str = string.Format("SELECT * FROM `data_items` WHERE `device_id` = '{0}' AND `agent_instance_id` = {1}",new object[]{ this.EscapeString(deviceId), agentInstanceId});
            return ReadList<DataItemDefinition>(str);
        }

        public DeviceDefinition ReadDevice(string deviceId, long agentInstanceId)
        {
            var str = string.Format("SELECT * FROM `devices` WHERE `device_id` = '{0}' AND `agent_instance_id` = {1} LIMIT 1", new object[] { this.EscapeString(deviceId), agentInstanceId });
            return Read<DeviceDefinition>(str);
        }

        /// <summary>
        /// get the a list of RejectedPart,注意:SQL语句格式错误，需要更改为正确的格式。
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="partIds"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="at"></param>
        /// <returns></returns>
        public List<RejectedPart> ReadRejectedParts(string deviceId, string[] partIds, DateTime from, DateTime to, DateTime at)
        {
            List<RejectedPart> list = new List<RejectedPart>();
            string str = "*";
            string str2 = "parts_rejected";
            string str3 = "";
            if ((partIds != null) && (partIds.Length != 0))
            {
                for (int i = 0; i < partIds.Length; i++)
                {
                    str3 = str3 + "`part_id`='" + partIds[i] + "'";
                    if (i < (partIds.Length - 1))
                    {
                        str3 = str3 + " OR ";
                    }
                }
                str3 = " AND ({str3}) ";
            }
            string str4 = null;
            if ((from > DateTime.MinValue) && (to > DateTime.MinValue))
            {
                str4 = "SELECT {str} FROM `{str2}` WHERE `device_id` = '{deviceId}'{str3} AND `timestamp` >= '{from.ToUnixTime()}' AND `timestamp` <= '{to.ToUnixTime()}'";
            }
            else if (from > DateTime.MinValue)
            {
                str4 = "SELECT {str} FROM `{str2}` WHERE `device_id` = '{deviceId}'{str3} AND `timestamp` >= '{from.ToUnixTime()}' LIMIT 1000";
            }
            else if (to > DateTime.MinValue)
            {
                str4 = "SELECT {str} FROM `{str2}` WHERE `device_id` = '{deviceId}'{str3} AND `timestamp` <= '{to.ToUnixTime()}' LIMIT 1000";
            }
            else if (at > DateTime.MinValue)
            {
                str4 = "SELECT {str} FROM `{str2}` WHERE `device_id` = '{deviceId}'{str3} AND `timestamp` = '{at.ToUnixTime()}'";
            }
            else
            {
                str4 = "SELECT {str} FROM `{str2}` WHERE `device_id` = '{deviceId}'{str3}";
            }
            if (!string.IsNullOrEmpty(str4))
            {
                list = ReadList<RejectedPart>(str4);
            }
            return list;
        }

        public List<Sample> ReadSamples(string[] dataItemIds, string deviceId, DateTime from, DateTime to, DateTime at, long count)
        {
            List<Sample> list = new List<Sample>();
            string str = "*";
            string str2 = "archived_samples";
            string str3 = "current_samples";
            string format = "CALL getInstance('{0}', {1})";
            string str5 = "";
            if ((dataItemIds != null) && (dataItemIds.Length != 0))
            {
                for (int i = 0; i < dataItemIds.Length; i++)
                {
                    str5 = str5 + "`id`='" + dataItemIds[i] + "'";
                    if (i < (dataItemIds.Length - 1))
                    {
                        str5 = str5 + " OR ";
                    }
                }
                str5 = str5 + " AND ";
            }
            List<string> list2 = new List<string>();
            if ((from > DateTime.MinValue) && (to > DateTime.MinValue))
            {
                list2.Add(string.Format(format, deviceId, from.ToUnixTime()));
                string str6 = "SELECT {0} FROM `{1}` WHERE {2}`device_id` = '{3}' AND `timestamp` >= '{4}' AND `timestamp` <= '{5}'";
                list2.Add(string.Format(str6, new object[] { str, str2, str5, deviceId, from.ToUnixTime(), to.ToUnixTime() }));
            }
            else if ((from > DateTime.MinValue) && (count > 0L))
            {
                list2.Add(string.Format(format, deviceId, from.ToUnixTime()));
                string str7 = "SELECT {0} FROM `{1}` WHERE {2}`device_id` = '{3}' AND `timestamp` >= '{4}' LIMIT {5}";
                list2.Add(string.Format(str7, new object[] { str, str2, str5, deviceId, from.ToUnixTime(), count }));
            }
            else if ((to > DateTime.MinValue) && (count > 0L))
            {
                list2.Add(string.Format(format, deviceId, to.ToUnixTime()));
                string str8 = "SELECT {0} FROM `{1}` WHERE {2}`device_id` = '{3}' AND `timestamp` <= '{4}' LIMIT {5}";
                list2.Add(string.Format(str8, new object[] { str, str2, str5, deviceId, to.ToUnixTime(), count }));
            }
            else if (from > DateTime.MinValue)
            {
                list2.Add(string.Format(format, deviceId, from.ToUnixTime()));
                string str9 = "SELECT {0} FROM `{1}` WHERE {2}`device_id` = '{3}' AND `timestamp` <= '{4}' LIMIT 1000";
                list2.Add(string.Format(str9, new object[] { str, str2, str5, deviceId, from.ToUnixTime() }));
            }
            else if (to > DateTime.MinValue)
            {
                list2.Add(string.Format(format, deviceId, to.ToUnixTime()));
                string str10 = "SELECT {0} FROM `{1}` WHERE {2}`device_id` = '{3}' AND `timestamp` <= '{4}' LIMIT 1000";
                list2.Add(string.Format(str10, new object[] { str, str2, str5, deviceId, to.ToUnixTime() }));
            }
            else if (count > 0L)
            {
                string str11 = "SELECT {0} FROM `{1}` WHERE {2}`device_id` = '{3}' ORDER BY `timestamp` DESC LIMIT {4}";
                list2.Add(string.Format(str11, new object[] { str, str2, str5, deviceId, count }));
            }
            else if (at > DateTime.MinValue)
            {
                list2.Add(string.Format(format, deviceId, at.ToUnixTime()));
            }
            else
            {
                string str12 = "SELECT {0} FROM `{1}` WHERE {2}`device_id` = '{3}'";
                list2.Add(string.Format(str12, new object[] { str, str3, str5, deviceId, at.ToUnixTime() }));
            }
            foreach (string str13 in list2)
            {
                list.AddRange(ReadList<Sample>(str13));
            }
            if (!dataItemIds.IsNullOrEmpty<string>())
            {
                list = list.FindAll(o => dataItemIds.ToList<string>().Exists(x => x == o.Id));
            }
            return list;
        }

        public Status ReadStatus(string deviceId)
        {
            var str =string.Format( "SELECT * FROM `status` WHERE `device_id` = '{0}' LIMIT 1",deviceId);
            return Read<Status>(str);
        }

        

        /// <summary>
        /// get the list of VerifiedParts,注意：函数中的SQL语句参数格式错误，请更正
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="partIds"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="at"></param>
        /// <returns></returns>
        public List<VerifiedPart> ReadVerifiedParts(string deviceId, string[] partIds, DateTime from, DateTime to, DateTime at)
        {
            List<VerifiedPart> list = new List<VerifiedPart>();
            string str = "*";
            string str2 = "parts_verified";
            string str3 = "";
            if ((partIds != null) && (partIds.Length != 0))
            {
                for (int i = 0; i < partIds.Length; i++)
                {
                    str3 = str3 + "`part_id`='" + partIds[i] + "'";
                    if (i < (partIds.Length - 1))
                    {
                        str3 = str3 + " OR ";
                    }
                }
                str3 = " AND ({str3}) ";
            }
            string str4 = null;
            if ((from > DateTime.MinValue) && (to > DateTime.MinValue))
            {
                str4 = "SELECT {str} FROM `{str2}` WHERE `device_id` = '{deviceId}'{str3} AND `timestamp` >= '{from.ToUnixTime()}' AND `timestamp` <= '{to.ToUnixTime()}'";
            }
            else if (from > DateTime.MinValue)
            {
                str4 = "SELECT {str} FROM `{str2}` WHERE `device_id` = '{deviceId}'{str3} AND `timestamp` >= '{from.ToUnixTime()}' LIMIT 1000";
            }
            else if (to > DateTime.MinValue)
            {
                str4 = "SELECT {str} FROM `{str2}` WHERE `device_id` = '{deviceId}'{str3} AND `timestamp` <= '{to.ToUnixTime()}' LIMIT 1000";
            }
            else if (at > DateTime.MinValue)
            {
                str4 = "SELECT {str} FROM `{str2}` WHERE `device_id` = '{deviceId}'{str3} AND `timestamp` = '{at.ToUnixTime()}'";
            }
            else
            {
                str4 = "SELECT {str} FROM `{str2}` WHERE `device_id` = '{deviceId}'{str3}";
            }
            if (!string.IsNullOrEmpty(str4))
            {
                list = ReadList<VerifiedPart>(str4);
            }
            return list;
        }

#endregion

        #region "Write"
        //wrtire to database
        private bool Write(string query)
        {

            if (!string.IsNullOrEmpty(query))
            {
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();

                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            return (command.ExecuteNonQuery() >= 0);
                        }
                    }
                }
                catch (NullReferenceException exception)
                {
                    logger.Debug<NullReferenceException>(exception);
                }
                catch (TimeoutException exception2)
                {
                    logger.Debug<TimeoutException>(exception2);
                }
                catch (MySqlException exception3)
                {
                    logger.Warn<MySqlException>(exception3);
                }
                catch (Exception exception4)
                {
                    logger.Error<Exception>(exception4);
                }
            }
            return false;
        }

        public bool Write(List<StatusData> definitions)
        {
            if (definitions.IsNullOrEmpty<StatusData>())
            {
                return false;
            }
            string str = "`device_id`, `timestamp`, `connected`, `available`";
            string format = "INSERT INTO `status` ({0}) VALUES {1} ON DUPLICATE KEY UPDATE `timestamp`=VALUES(`timestamp`), `connected`=VALUES(`connected`), `available`=VALUES(`available`)";
            string str3 = "('{0}',{1},{2},{3})";
            //string COLUMNS = "`device_id`, `timestamp`, `connected`, `available`";
            //string VALUES = "(@deviceId, @timestamp, @connected, @available)";
            //string QUERY_FORMAT = "INSERT INTO `status` ({0}) VALUES {1} ON DUPLICATE KEY UPDATE  `timestamp`=VALUES(`timestamp`), `connected`=VALUES(`connected`), `available`=VALUES(`available`)";
            string[] strArray = new string[definitions.Count];
            for (int i = 0; i < definitions.Count; i++)
            {
                StatusData data = definitions[i];
                strArray[i] = string.Format(str3, new object[] { this.EscapeString(data.DeviceId), data.Timestamp.ToUnixTime(), data.Connected ? 1 : 0, data.Available ? 1 : 0 });
            }
            string str4 = string.Join(",", strArray);
            string query = string.Format(format, str, str4);
            return this.Write(query);
        }

        public bool Write(List<VerifiedPart> parts)
        {
            if (parts.IsNullOrEmpty<VerifiedPart>())
            {
                return false;
            }
            string str = "`device_id`, `part_id`, `timestamp`, `message`";
            string format = "INSERT IGNORE INTO `parts_verified` ({0}) VALUES {1}";
            string str3 = "('{0}','{1}',{2},'{3}')";
            string[] strArray = new string[parts.Count];
            for (int i = 0; i < parts.Count; i++)
            {
                VerifiedPart part = parts[i];
                strArray[i] = string.Format(str3, new object[] { this.EscapeString(part.DeviceId), this.EscapeString(part.PartId), part.Timestamp.ToUnixTime(), this.EscapeString(part.Message) });
            }
            string str4 = string.Join(",", strArray);
            string query = string.Format(format, str, str4);
            return this.Write(query);
        }

        public bool Write(List<RejectedPart> parts)
        {
            if (parts.IsNullOrEmpty<RejectedPart>())
            {
                return false;
            }
            string str = "`device_id`, `part_id`, `timestamp`, `message`";
            string format = "INSERT IGNORE INTO `parts_rejected` ({0}) VALUES {1} ON DUPLICATE KEY UPDATE {2}";
            string str3 = "('{0}','{1}',{2},'{3}')";
            string str4 = "`timestamp`={0},`message`='{1}'";
            List<string> values = new List<string>();
            foreach (RejectedPart part in parts)
            {
                string str6 = string.Format(str3, new object[] { this.EscapeString(part.DeviceId), this.EscapeString(part.PartId), part.Timestamp.ToUnixTime(), this.EscapeString(part.Message) });
                string str7 = string.Format(str4, part.Timestamp.ToUnixTime(), this.EscapeString(part.Message));
                values.Add(string.Format(format, str, str6, str7));
            }
            string query = string.Join(";", values);
            return this.Write(query);
        }

        public bool Write(List<SampleData> samples)
        {
            if (samples.IsNullOrEmpty<SampleData>())
            {
                return false;
            }
            string str = "`device_id`, `id`, `timestamp`, `agent_instance_id`, `sequence`, `cdata`, `condition`";
            string format = "INSERT IGNORE INTO `archived_samples` ({0}) VALUES {1}";
            string str3 = "INSERT IGNORE INTO `current_samples` ({0}) VALUES {1} ON DUPLICATE KEY UPDATE {2}";
            string str4 = "('{0}','{1}',{2},{3},{4},'{5}','{6}')";
            string str5 = "`timestamp`={0},`agent_instance_id`={1},`sequence`={2},`cdata`='{3}',`condition`='{4}'";
            string str6 = "";
            List<SampleData> list = samples.FindAll(o => o.StreamDataType == StreamDataType.ARCHIVED_SAMPLE);
            for (int i = 0; i < list.Count; i++)
            {
                SampleData data = list[i];
                str6 = str6 + string.Format(str4, new object[] { this.EscapeString(data.DeviceId), this.EscapeString(data.Id), data.Timestamp.ToUnixTime(), data.AgentInstanceId, data.Sequence, this.EscapeString(data.CDATA), this.EscapeString(data.Condition) });
                if (i < (list.Count - 1))
                {
                    str6 = str6 + ",";
                }
            }
            List<string> collection = new List<string>();
            using (IEnumerator<string> enumerator = (from o in samples select o.Id).Distinct<string>().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    string id = enumerator.Current;
                    SampleData data2 = (from o in samples
                                        orderby o.Timestamp
                                        select o).ToList<SampleData>().First<SampleData>(o => o.Id == id);
                    if (data2 != null)
                    {
                        string str8 = string.Format(str4, new object[] { this.EscapeString(data2.DeviceId), this.EscapeString(data2.Id), data2.Timestamp.ToUnixTime(), data2.AgentInstanceId, data2.Sequence, this.EscapeString(data2.CDATA), this.EscapeString(data2.Condition) });
                        string str9 = string.Format(str5, new object[] { data2.Timestamp.ToUnixTime(), data2.AgentInstanceId, data2.Sequence, this.EscapeString(data2.CDATA), this.EscapeString(data2.Condition) });
                        collection.Add(string.Format(str3, str, str8, str9));
                    }
                }
            }
            List<string> values = new List<string>();
            if (list.Count > 0)
            {
                values.Add(string.Format(format, str, str6));
            }
            values.AddRange(collection);
            string query = string.Join(";", values);
            return this.Write(query);
        }

        public bool Write(List<DeviceDefinitionData> definitions)
        {
            if (definitions.IsNullOrEmpty<DeviceDefinitionData>())
            {
                return false;
            }
            string str = "`device_id`, `agent_instance_id`, `id`, `uuid`, `name`, `native_name`, `sample_interval`, `sample_rate`, `iso_841_class`, `manufacturer`, `model`, `serial_number`, `station`, `description`";
            string format = "INSERT IGNORE INTO `devices` ({0}) VALUES {1}";
            string str3 = "('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}')";
            string[] strArray = new string[definitions.Count];
            for (int i = 0; i < definitions.Count; i++)
            {
                DeviceDefinitionData data = definitions[i];
                strArray[i] = string.Format(str3, new object[] { this.EscapeString(data.DeviceId), data.AgentInstanceId, 
                    this.EscapeString(data.Id), this.EscapeString(data.Uuid), this.EscapeString(data.Name), 
                    this.EscapeString(data.NativeName), data.SampleInterval, data.SampleRate, this.EscapeString(data.Iso841Class), 
                    this.EscapeString(data.Manufacturer), this.EscapeString(data.Model), this.EscapeString(data.SerialNumber), 
                    this.EscapeString(data.Station), this.EscapeString(data.Description) });
            }
            string str4 = string.Join(",", strArray);
            string query = string.Format(format, str, str4);
            return this.Write(query);
        }

        public bool Write(List<DataItemDefinitionData> definitions)
        {
            if (definitions.IsNullOrEmpty<DataItemDefinitionData>())
            {
                return false;
            }
            string str = "`device_id`,`agent_instance_id`, `id`, `name`, `category`, `type`, `sub_type`, `statistic`, `units`,`native_units`,`native_scale`,`coordinate_system`,`sample_rate`,`representation`,`significant_digits`,`parent_id`";
            string format = "INSERT IGNORE INTO `data_items` ({0}) VALUES {1}";
            string str3 = "('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}')";
            string[] strArray = new string[definitions.Count];
            for (int i = 0; i < definitions.Count; i++)
            {
                DataItemDefinitionData data = definitions[i];
                strArray[i] = string.Format(str3, new object[] { this.EscapeString(data.DeviceId), data.AgentInstanceId, data.Id, this.EscapeString(data.Name), this.EscapeString(data.Category), this.EscapeString(data.Type), this.EscapeString(data.SubType), this.EscapeString(data.Statistic), this.EscapeString(data.Units), this.EscapeString(data.NativeUnits), this.EscapeString(data.NativeScale), this.EscapeString(data.CoordinateSystem), data.SampleRate, this.EscapeString(data.Representation), data.SignificantDigits, this.EscapeString(data.ParentId) });
            }
            string str4 = string.Join(",", strArray);
            string query = string.Format(format, str, str4);
            return this.Write(query);
        }

        public bool Write(List<ComponentDefinitionData> definitions)
        {
            if (definitions.IsNullOrEmpty<ComponentDefinitionData>())
            {
                return false;
            }
            string str = "`device_id`,`agent_instance_id`, `id`, `uuid`, `name`, `native_name`, `sample_interval`, `sample_rate`, `type`,`parent_id`";
            string format = "INSERT IGNORE INTO `components` ({0}) VALUES {1}";
            string str3 = "('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}')";
            string[] strArray = new string[definitions.Count];
            for (int i = 0; i < definitions.Count; i++)
            {
                ComponentDefinitionData data = definitions[i];
                strArray[i] = string.Format(str3, new object[] { this.EscapeString(data.DeviceId), data.AgentInstanceId, 
                    this.EscapeString(data.Id), this.EscapeString(data.Uuid), this.EscapeString(data.Name), 
                    this.EscapeString(data.NativeName), data.SampleInterval, data.SampleRate, this.EscapeString(data.Type), 
                    this.EscapeString(data.ParentId) });
            }
            string str4 = string.Join(",", strArray);
            string query = string.Format(format, str, str4);
            return this.Write(query);
        }

        public bool Write(List<AssetDefinitionData> definitions)
        {
            if (definitions.IsNullOrEmpty<AssetDefinitionData>())
            {
                return false;
            }
            string str = "`device_id`, `id`, `timestamp`, `agent_instance_id`, `type`, `xml`";
            string format = "INSERT IGNORE INTO `assets` ({0}) VALUES {1}";
            string str3 = "('{0}','{1}',{2},{3},'{4}','{5}')";
            string[] strArray = new string[definitions.Count];
            for (int i = 0; i < definitions.Count; i++)
            {
                AssetDefinitionData data = definitions[i];
                strArray[i] = string.Format(str3, new object[] { this.EscapeString(data.DeviceId), this.EscapeString(data.Id), data.Timestamp.ToUnixTime(), data.AgentInstanceId, this.EscapeString(data.Type), this.EscapeString(data.Xml) });
            }
            string str4 = string.Join(",", strArray);
            string query = string.Format(format, str, str4);
            return this.Write(query);
        }

        public bool Write(List<AgentDefinitionData> definitions)
        {
            if (definitions.IsNullOrEmpty<AgentDefinitionData>())
            {
                return false;
            }
            string str = "`device_id`, `instance_id`, `sender`, `version`, `buffer_size`, `test_indicator`, `timestamp`";
            string format = "INSERT IGNORE INTO `agents` ({0}) VALUES {1}";
            string str3 = "('{0}','{1}','{2}','{3}','{4}','{5}','{6}')";
            string[] strArray = new string[definitions.Count];
            for (int i = 0; i < definitions.Count; i++)
            {
                AgentDefinitionData data = definitions[i];
                strArray[i] = string.Format(str3, new object[] { this.EscapeString(data.DeviceId), data.InstanceId, this.EscapeString(data.Sender), this.EscapeString(data.Version), data.BufferSize, this.EscapeString(data.TestIndicator), data.Timestamp.ToUnixTime() });
            }
            string str4 = string.Join(",", strArray);
            string query = string.Format(format, str, str4);
            return this.Write(query);
        }

        public bool Write(List<ConnectionDefinitionData> definitions)
        {
            //Write ConnectionDefinitionData to Database
            if (definitions.IsNullOrEmpty<ConnectionDefinitionData>())
            {
                return false;
            }

            string str = "`device_id`, `address`, `port`, `physical_address`";
            string format = "INSERT INTO `connections` ({0}) VALUES {1} ON DUPLICATE KEY UPDATE `address`=VALUES(`address`), `port`=VALUES(`port`), `physical_address`=VALUES(`physical_address`)";
            string str3 = "('{0}','{1}',{2},'{3}')";
            string[] strArray = new string[definitions.Count];
            for (int i = 0; i < definitions.Count; i++)
            {

                ConnectionDefinitionData data = definitions[i];
                strArray[i] = string.Format(str3, new object[] { this.EscapeString(data.DeviceId), this.EscapeString(data.Address), data.Port, this.EscapeString(data.PhysicalAddress) });
            }
            string str4 = string.Join(",", strArray);
            string query = string.Format(format, str, str4);


            return this.Write(query);
        }
    }
    
    
}

        #endregion