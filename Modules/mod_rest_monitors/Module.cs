using System;
using System.Collections.Generic;
using NLog;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using System.Threading;
using TrakHound.Api.v2;
using Json = TrakHound.Api.v2.Json;

namespace mod_rest_monitors
{
    [InheritedExport(typeof(IRestModule))]
    public class Module : IRestModule
    {
        //日志
        private static Logger log = LogManager.GetCurrentClassLogger();

        //获取接口名称
        public string Name
        {
            get { return "monitors"; }
        }

        //请求的响应
        public bool GetResponse(Uri requestUri, Stream stream)
        {
            var query = new RequestQuery(requestUri);

            if (query.IsValid)
            {
                log.Info("Monitors Request Received : " + query.DeviceId);

                try
                {
                    var sent = new List<Item>();
                    DateTime from = query.From;

                    //如果DeviceId不为空，则读取DevcieId的Connection连接
                    if (!string.IsNullOrEmpty(query.DeviceId))
                    {
                        var connection = Database.ReadConnection(query.DeviceId);
                        if (connection != null)
                        {
                            bool write = true;
                            var deviceId = connection.DeviceId;

                            //新建监控数据对象
                            DeviceMonitorData monitorData = new DeviceMonitorData();

                            if (!string.IsNullOrEmpty(deviceId))
                            {
                                monitorData.DeviceId = deviceId;
                                monitorData.Address = connection.Address;
                                monitorData.Port = connection.Port;

                                //数据库检索Status
                                var status = Database.ReadStatus(deviceId);
                                if (status != null)
                                {
                                    monitorData.Available = status.Available;
                                    monitorData.Connected = status.Connected;
                                }

                                //如果机床Available可用并且Connected上代理进行数据的检索
                                if (monitorData.Available && monitorData.Connected)
                                {
                                    //数据库检索Sample
                                    var samples = Database.ReadSamples(query.DataItems, deviceId, from, query.To, query.At, query.Count);

                                    if (!samples.IsNullOrEmpty())
                                    {
                                        foreach (var sample in samples)
                                        {

                                            monitorData.AgentInstanceId = sample.AgentInstanceId;
                                            monitorData.Timestamp = sample.Timestamp;

                                            //根据约定的数据的id命名规则来判断数据项具体属于DeviceMonitorData的哪一个成员
                                            //急停
                                            if (sample.Id.Contains("_estop"))
                                            {
                                                monitorData.EStop = sample.CDATA;
                                            }
                                            else
                                            {
                                                monitorData.EStop = "UNAVAILABLE";
                                            }

                                            //控制器模式
                                            if (sample.Id.Contains("_controller_mode"))
                                            {
                                                monitorData.ControllerMode = sample.CDATA;
                                            }
                                            else
                                            {
                                                monitorData.ControllerMode = "UNAVAILABLE";
                                            }

                                            //系统报警
                                            if (sample.Id.Contains("_system"))
                                            {
                                                monitorData.System = sample.CDATA;
                                            }
                                            else
                                            {
                                                monitorData.System = "UNAVAILABLE";
                                            }

                                            //程序执行状态
                                            if (sample.Id.Contains("_execution"))
                                            {
                                                monitorData.Execution = sample.CDATA;
                                            }
                                            else
                                            {
                                                monitorData.Execution = "UNAVAILABLE";
                                            }

                                            //程序名
                                            if (sample.Id.Contains("_program"))
                                            {
                                                monitorData.Execution = sample.CDATA;
                                            }
                                            else
                                            {
                                                monitorData.Program = "UNAVAILABLE";
                                            }

                                            //主轴转速
                                            if (sample.Id.Contains("_spindle_velocity"))
                                            {
                                                monitorData.SpindleVelocity = sample.CDATA;
                                            }
                                            else
                                            {
                                                monitorData.SpindleVelocity = "UNAVAILABLE";
                                            }

                                            //主轴负载
                                            if (sample.Id.Contains("spindle_load"))
                                            {
                                                monitorData.SpindleLoad = sample.CDATA;
                                            }
                                            else
                                            {
                                                monitorData.SpindleLoad = "UNAVAILABLE";
                                            }

                                            //进给速度
                                            if (sample.Id.Contains("_feedrate"))
                                            {
                                                monitorData.Feedrate = sample.CDATA;
                                            }
                                            else
                                            {
                                                monitorData.Feedrate = "UNAVAILABLE";
                                            }

                                            //XYZABC进给轴位置
                                            if (sample.Id.Contains("_xpos"))
                                            {
                                                monitorData.XPos = sample.CDATA;
                                            }
                                            else
                                            {
                                                monitorData.XPos = "UNAVAILABLE";
                                            }

                                            if (sample.Id.Contains("_ypos"))
                                            {
                                                monitorData.YPos = sample.CDATA;
                                            }
                                            else
                                            {
                                                monitorData.YPos = "UNAVAILABLE";
                                            }

                                            if (sample.Id.Contains("_zpos"))
                                            {
                                                monitorData.ZPos = sample.CDATA;
                                            }
                                            else
                                            {
                                                monitorData.ZPos = "UNAVAILABLE";
                                            }

                                            if (sample.Id.Contains("_aangle"))
                                            {
                                                monitorData.AAngle = sample.CDATA;
                                            }
                                            else
                                            {
                                                monitorData.AAngle = "UNAVAILABLE";
                                            }

                                            if (sample.Id.Contains("_bangle"))
                                            {
                                                monitorData.BAngle = sample.CDATA;
                                            }
                                            else
                                            {
                                                monitorData.BAngle = "UNAVAILABLE";
                                            }

                                            if (sample.Id.Contains("_cangle"))
                                            {
                                                monitorData.CAngle = sample.CDATA;
                                            }
                                            else
                                            {
                                                monitorData.CAngle = "UNAVAILABLE";
                                            }

                                            //XYZABC进给轴速度
                                            if (sample.Id.Contains("_xvelocity"))
                                            {
                                                monitorData.XVelocity = sample.CDATA;
                                            }
                                            else
                                            {
                                                monitorData.XVelocity = "UNAVAILABLE";
                                            }

                                            if (sample.Id.Contains("_yvelocity"))
                                            {
                                                monitorData.YVelocity = sample.CDATA;
                                            }
                                            else
                                            {
                                                monitorData.YVelocity = "UNAVAILABLE";
                                            }

                                            if (sample.Id.Contains("_zvelocity"))
                                            {
                                                monitorData.ZVelocity = sample.CDATA;
                                            }
                                            else
                                            {
                                                monitorData.ZVelocity = "UNAVAILABLE";
                                            }

                                            if (sample.Id.Contains("_arotary_velocity"))
                                            {
                                                monitorData.ARotaryVelocity = sample.CDATA;
                                            }
                                            else
                                            {
                                                monitorData.ARotaryVelocity = "UNAVAILABLE";
                                            }

                                            if (sample.Id.Contains("_brotary_velocity"))
                                            {
                                                monitorData.BRotaryVelocity = sample.CDATA;
                                            }
                                            else
                                            {
                                                monitorData.BRotaryVelocity = "UNAVAILABLE";
                                            }

                                            if (sample.Id.Contains("_crotary_velocity"))
                                            {
                                                monitorData.CRotaryVelocity = sample.CDATA;
                                            }
                                            else
                                            {
                                                monitorData.CRotaryVelocity = "UNAVAILABLE";
                                            }

                                            //XYZABC进给轴负载
                                            if (sample.Id.Contains("_xload"))
                                            {
                                                monitorData.XLoad = sample.CDATA;
                                            }
                                            else
                                            {
                                                monitorData.XLoad = "UNAVAILABLE";
                                            }

                                            if (sample.Id.Contains("_yload"))
                                            {
                                                monitorData.YLoad = sample.CDATA;
                                            }
                                            else
                                            {
                                                monitorData.YLoad = "UNAVAILABLE";
                                            }

                                            if (sample.Id.Contains("_zload"))
                                            {
                                                monitorData.ZLoad = sample.CDATA;
                                            }
                                            else
                                            {
                                                monitorData.ZLoad = "UNAVAILABLE";
                                            }

                                            if (sample.Id.Contains("_aload"))
                                            {
                                                monitorData.ALoad = sample.CDATA;
                                            }
                                            else
                                            {
                                                monitorData.ALoad = "UNAVAILABLE";
                                            }

                                            if (sample.Id.Contains("_bload"))
                                            {
                                                monitorData.BLoad = sample.CDATA;
                                            }
                                            else
                                            {
                                                monitorData.BLoad = "UNAVAILABLE";
                                            }

                                            if (sample.Id.Contains("_cload"))
                                            {
                                                monitorData.CLoad = sample.CDATA;
                                            }
                                            else
                                            {
                                                monitorData.CLoad = "UNAVAILABLE";
                                            }

                                            //传感器功率、能耗
                                            if (sample.Id.Contains("_power"))
                                            {
                                                monitorData.Power = sample.CDATA;
                                            }
                                            else
                                            {
                                                monitorData.Power = "UNAVAILABLE";
                                            }

                                            if (sample.Id.Contains("_energy_consumption"))
                                            {
                                                monitorData.EnergyConsumption = sample.CDATA;
                                            }
                                            else
                                            {
                                                monitorData.EnergyConsumption = "UNAVAILABLE";
                                            }

                                            //传感器监测的机床颤振信息
                                            if (sample.Id.Contains("_chatter_vibration"))
                                            {
                                                monitorData.ChatterVibration = sample.CDATA;
                                            }
                                            else
                                            {
                                                monitorData.ChatterVibration = "UNAVAILABLE";
                                            }
                                        }
                                    }
                                }




                            }

                            var item = new Item(monitorData);
                            sent.Add(item);

                            if (write)
                            {
                                string json = Json.Convert.ToJson(item, true);
                                var bytes = Encoding.UTF8.GetBytes(json);
                                stream.Write(bytes, 0, bytes.Length);
                                stream.Flush();
                            }
                            else stream.WriteByte(32);
                            
                        }
                    }
                    else
                    {
                        var connections = Database.ReadConnections();
                        if (connections != null)
                        {
                            
                            //根据连接的代理来获取数据
                            foreach (var connection in connections)
                            {
                                bool write = true;
                                var deviceId = connection.DeviceId;

                                //新建监控数据对象
                                DeviceMonitorData monitorData = new DeviceMonitorData();

                                if (!string.IsNullOrEmpty(deviceId))
                                {
                                    monitorData.DeviceId = deviceId;
                                    monitorData.Address = connection.Address;
                                    monitorData.Port = connection.Port;

                                    //数据库检索Status
                                    var status = Database.ReadStatus(deviceId);
                                    if (status != null)
                                    {
                                        monitorData.Available = status.Available;
                                        monitorData.Connected = status.Connected;
                                    }

                                    //如果机床Available可用并且Connected上代理进行数据的检索
                                    if (monitorData.Available && monitorData.Connected)
                                    {
                                        //数据库检索Sample
                                        var samples = Database.ReadSamples(query.DataItems, deviceId, from, query.To, query.At, query.Count);

                                        if (!samples.IsNullOrEmpty())
                                        {
                                            foreach (var sample in samples)
                                            {

                                                monitorData.AgentInstanceId = sample.AgentInstanceId;
                                                monitorData.Timestamp = sample.Timestamp;

                                                //根据约定的数据的id命名规则来判断数据项具体属于DeviceMonitorData的哪一个成员
                                                //急停
                                                if (sample.Id.Contains("_estop"))
                                                {
                                                    monitorData.EStop = sample.CDATA;
                                                }
                                                else
                                                {
                                                    monitorData.EStop = "UNAVAILABLE";
                                                }

                                                //控制器模式
                                                if (sample.Id.Contains("_controller_mode"))
                                                {
                                                    monitorData.ControllerMode = sample.CDATA;
                                                }
                                                else
                                                {
                                                    monitorData.ControllerMode = "UNAVAILABLE";
                                                }

                                                //系统报警
                                                if (sample.Id.Contains("_system"))
                                                {
                                                    monitorData.System = sample.CDATA;
                                                }
                                                else
                                                {
                                                    monitorData.System = "UNAVAILABLE";
                                                }

                                                //程序执行状态
                                                if (sample.Id.Contains("_execution"))
                                                {
                                                    monitorData.Execution = sample.CDATA;
                                                }
                                                else
                                                {
                                                    monitorData.Execution = "UNAVAILABLE";
                                                }

                                                //程序名
                                                if (sample.Id.Contains("_program"))
                                                {
                                                    monitorData.Execution = sample.CDATA;
                                                }
                                                else
                                                {
                                                    monitorData.Program = "UNAVAILABLE";
                                                }

                                                //主轴转速
                                                if (sample.Id.Contains("_spindle_velocity"))
                                                {
                                                    monitorData.SpindleVelocity = sample.CDATA;
                                                }
                                                else
                                                {
                                                    monitorData.SpindleVelocity = "UNAVAILABLE";
                                                }

                                                //主轴负载
                                                if (sample.Id.Contains("spindle_load"))
                                                {
                                                    monitorData.SpindleLoad = sample.CDATA;
                                                }
                                                else
                                                {
                                                    monitorData.SpindleLoad = "UNAVAILABLE";
                                                }

                                                //进给速度
                                                if (sample.Id.Contains("_feedrate"))
                                                {
                                                    monitorData.Feedrate = sample.CDATA;
                                                }
                                                else
                                                {
                                                    monitorData.Feedrate = "UNAVAILABLE";
                                                }

                                                //XYZABC进给轴位置
                                                if (sample.Id.Contains("_xpos"))
                                                {
                                                    monitorData.XPos = sample.CDATA;
                                                }
                                                else
                                                {
                                                    monitorData.XPos = "UNAVAILABLE";
                                                }

                                                if (sample.Id.Contains("_ypos"))
                                                {
                                                    monitorData.YPos = sample.CDATA;
                                                }
                                                else
                                                {
                                                    monitorData.YPos = "UNAVAILABLE";
                                                }

                                                if (sample.Id.Contains("_zpos"))
                                                {
                                                    monitorData.ZPos = sample.CDATA;
                                                }
                                                else
                                                {
                                                    monitorData.ZPos = "UNAVAILABLE";
                                                }

                                                if (sample.Id.Contains("_aangle"))
                                                {
                                                    monitorData.AAngle = sample.CDATA;
                                                }
                                                else
                                                {
                                                    monitorData.AAngle = "UNAVAILABLE";
                                                }

                                                if (sample.Id.Contains("_bangle"))
                                                {
                                                    monitorData.BAngle = sample.CDATA;
                                                }
                                                else
                                                {
                                                    monitorData.BAngle = "UNAVAILABLE";
                                                }

                                                if (sample.Id.Contains("_cangle"))
                                                {
                                                    monitorData.CAngle = sample.CDATA;
                                                }
                                                else
                                                {
                                                    monitorData.CAngle = "UNAVAILABLE";
                                                }

                                                //XYZABC进给轴速度
                                                if (sample.Id.Contains("_xvelocity"))
                                                {
                                                    monitorData.XVelocity = sample.CDATA;
                                                }
                                                else
                                                {
                                                    monitorData.XVelocity = "UNAVAILABLE";
                                                }

                                                if (sample.Id.Contains("_yvelocity"))
                                                {
                                                    monitorData.YVelocity = sample.CDATA;
                                                }
                                                else
                                                {
                                                    monitorData.YVelocity = "UNAVAILABLE";
                                                }

                                                if (sample.Id.Contains("_zvelocity"))
                                                {
                                                    monitorData.ZVelocity = sample.CDATA;
                                                }
                                                else
                                                {
                                                    monitorData.ZVelocity = "UNAVAILABLE";
                                                }

                                                if (sample.Id.Contains("_arotary_velocity"))
                                                {
                                                    monitorData.ARotaryVelocity = sample.CDATA;
                                                }
                                                else
                                                {
                                                    monitorData.ARotaryVelocity = "UNAVAILABLE";
                                                }

                                                if (sample.Id.Contains("_brotary_velocity"))
                                                {
                                                    monitorData.BRotaryVelocity = sample.CDATA;
                                                }
                                                else
                                                {
                                                    monitorData.BRotaryVelocity = "UNAVAILABLE";
                                                }

                                                if (sample.Id.Contains("_crotary_velocity"))
                                                {
                                                    monitorData.CRotaryVelocity = sample.CDATA;
                                                }
                                                else
                                                {
                                                    monitorData.CRotaryVelocity = "UNAVAILABLE";
                                                }

                                                //XYZABC进给轴负载
                                                if (sample.Id.Contains("_xload"))
                                                {
                                                    monitorData.XLoad = sample.CDATA;
                                                }
                                                else
                                                {
                                                    monitorData.XLoad = "UNAVAILABLE";
                                                }

                                                if (sample.Id.Contains("_yload"))
                                                {
                                                    monitorData.YLoad = sample.CDATA;
                                                }
                                                else
                                                {
                                                    monitorData.YLoad = "UNAVAILABLE";
                                                }

                                                if (sample.Id.Contains("_zload"))
                                                {
                                                    monitorData.ZLoad = sample.CDATA;
                                                }
                                                else
                                                {
                                                    monitorData.ZLoad = "UNAVAILABLE";
                                                }

                                                if (sample.Id.Contains("_aload"))
                                                {
                                                    monitorData.ALoad = sample.CDATA;
                                                }
                                                else
                                                {
                                                    monitorData.ALoad = "UNAVAILABLE";
                                                }

                                                if (sample.Id.Contains("_bload"))
                                                {
                                                    monitorData.BLoad = sample.CDATA;
                                                }
                                                else
                                                {
                                                    monitorData.BLoad = "UNAVAILABLE";
                                                }

                                                if (sample.Id.Contains("_cload"))
                                                {
                                                    monitorData.CLoad = sample.CDATA;
                                                }
                                                else
                                                {
                                                    monitorData.CLoad = "UNAVAILABLE";
                                                }

                                                //传感器功率、能耗
                                                if (sample.Id.Contains("_power"))
                                                {
                                                    monitorData.Power = sample.CDATA;
                                                }
                                                else
                                                {
                                                    monitorData.Power = "UNAVAILABLE";
                                                }

                                                if (sample.Id.Contains("_energy_consumption"))
                                                {
                                                    monitorData.EnergyConsumption = sample.CDATA;
                                                }
                                                else
                                                {
                                                    monitorData.EnergyConsumption = "UNAVAILABLE";
                                                }

                                                //传感器监测的机床颤振信息
                                                if (sample.Id.Contains("_chatter_vibration"))
                                                {
                                                    monitorData.ChatterVibration = sample.CDATA;
                                                }
                                                else
                                                {
                                                    monitorData.ChatterVibration = "UNAVAILABLE";
                                                }
                                            }
                                        }
                                    }
                                    

                                    

                                }

                                var item=new Item(monitorData);
                                sent.Add(item);

                                if (write)
                                {
                                    string json = Json.Convert.ToJson(item,true);
                                    json += "\r\n";
                                    var bytes = Encoding.UTF8.GetBytes(json);
                                    stream.Write(bytes, 0, bytes.Length);
                                    stream.Flush();
                                }
                                else stream.WriteByte(32);
                            }
                        }
                        if (from > DateTime.MinValue) from = DateTime.UtcNow;

                        if (query.Interval <= 0)
                        {

                        }
                        else Thread.Sleep(query.Interval);

                    }
                    return true;
                }
                catch (Exception ex)
                {
                    log.Trace(ex);
                }
            }

            return false;
        }

        public bool SendData(Uri requestUri, Stream stream)
        {
            return false;
        }

        public bool DeleteData(Uri requestUri)
        {
            return false;
        }

    }
}
