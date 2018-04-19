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

                            initDeviceMonitorData(monitorData);

                            if (!string.IsNullOrEmpty(deviceId))
                            {
                                monitorData.DeviceId = deviceId;
                                monitorData.Address = connection.Address;
                                monitorData.Port = connection.Port;

                                //从数据库检索Agent代理信息
                                var agent = Database.ReadAgent(deviceId);
                                if (agent != null)
                                {
                                    monitorData.AgentInstanceId = agent.InstanceId;
                                    monitorData.Timestamp = agent.Timestamp;
                                }

                                //检索Device信息，此处检索name和uuid
                                var device = Database.ReadDevice(deviceId, monitorData.AgentInstanceId);
                                if(device!=null)
                                {
                                    monitorData.Name = device.Name;
                                    monitorData.Uuid = device.Uuid;
                                }

                                //数据库检索Status
                                var status = Database.ReadStatus(deviceId);
                                if (status != null)
                                {
                                    monitorData.Available = status.Available;
                                    monitorData.Connected = status.Connected;
                                    monitorData.Timestamp = status.Timestamp;
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


                                            //根据约定的数据的id命名规则来判断数据项具体属于DeviceMonitorData的哪一个成员
                                            //急停
                                            if (sample.Id.Contains("_estop"))
                                            {
                                                monitorData.EStop = sample.CDATA;
                                            }


                                            //控制器模式
                                            if (sample.Id.Contains("_controller_mode"))
                                            {
                                                monitorData.ControllerMode = sample.CDATA;
                                            }


                                            //系统报警状态Fault、Warning等
                                            if (sample.Id.Contains("_system_status"))
                                            {
                                                monitorData.SystemStatus = sample.CDATA;
                                            }

                                            //系统报警具体信息
                                            if (sample.Id.Contains("_system_message"))
                                            {
                                                monitorData.SystemMessage = sample.CDATA;
                                            }

                                            //程序执行状态
                                            if (sample.Id.Contains("_execution"))
                                            {
                                                monitorData.Execution = sample.CDATA;
                                            }


                                            //程序名
                                            if (sample.Id.Contains("_program"))
                                            {
                                                monitorData.Execution = sample.CDATA;
                                            }


                                            //主轴转速
                                            if (sample.Id.Contains("_spindle_rotary_velocity"))
                                            {
                                                monitorData.SpindleRotaryVelocity = sample.CDATA;
                                            }


                                            //主轴负载
                                            if (sample.Id.Contains("_spindle_load"))
                                            {
                                                monitorData.SpindleLoad = sample.CDATA;
                                            }


                                            //进给速度
                                            if (sample.Id.Contains("_path_feedrate"))
                                            {
                                                monitorData.PathFeedrate = sample.CDATA;
                                            }


                                            //XYZABC进给轴位置
                                            if (sample.Id.Contains("_xpos"))
                                            {
                                                monitorData.XPos = sample.CDATA;
                                            }


                                            if (sample.Id.Contains("_ypos"))
                                            {
                                                monitorData.YPos = sample.CDATA;
                                            }


                                            if (sample.Id.Contains("_zpos"))
                                            {
                                                monitorData.ZPos = sample.CDATA;
                                            }


                                            if (sample.Id.Contains("_aangle"))
                                            {
                                                monitorData.AAngle = sample.CDATA;
                                            }


                                            if (sample.Id.Contains("_bangle"))
                                            {
                                                monitorData.BAngle = sample.CDATA;
                                            }


                                            if (sample.Id.Contains("_cangle"))
                                            {
                                                monitorData.CAngle = sample.CDATA;
                                            }


                                            //XYZABC进给轴速度
                                            if (sample.Id.Contains("_xvelocity"))
                                            {
                                                monitorData.XAxisFeedrate = sample.CDATA;
                                            }


                                            if (sample.Id.Contains("_yvelocity"))
                                            {
                                                monitorData.YAxisFeedrate = sample.CDATA;
                                            }


                                            if (sample.Id.Contains("_zvelocity"))
                                            {
                                                monitorData.ZAxisFeedrate = sample.CDATA;
                                            }


                                            if (sample.Id.Contains("_arotary_velocity"))
                                            {
                                                monitorData.ARotaryVelocity = sample.CDATA;
                                            }


                                            if (sample.Id.Contains("_brotary_velocity"))
                                            {
                                                monitorData.BRotaryVelocity = sample.CDATA;
                                            }


                                            if (sample.Id.Contains("_crotary_velocity"))
                                            {
                                                monitorData.CRotaryVelocity = sample.CDATA;
                                            }


                                            //XYZABC进给轴负载
                                            if (sample.Id.Contains("_xload"))
                                            {
                                                monitorData.XLoad = sample.CDATA;
                                            }


                                            if (sample.Id.Contains("_yload"))
                                            {
                                                monitorData.YLoad = sample.CDATA;
                                            }


                                            if (sample.Id.Contains("_zload"))
                                            {
                                                monitorData.ZLoad = sample.CDATA;
                                            }


                                            if (sample.Id.Contains("_aload"))
                                            {
                                                monitorData.ALoad = sample.CDATA;
                                            }


                                            if (sample.Id.Contains("_bload"))
                                            {
                                                monitorData.BLoad = sample.CDATA;
                                            }


                                            if (sample.Id.Contains("_cload"))
                                            {
                                                monitorData.CLoad = sample.CDATA;
                                            }


                                            //传感器功率、能耗
                                            if (sample.Id.Contains("_power"))
                                            {
                                                monitorData.Power = sample.CDATA;
                                            }

                                            if (sample.Id.Contains("_electrical_energy"))
                                            {
                                                monitorData.ElectricalEnergy = sample.CDATA;
                                            }


                                            //传感器监测的机床颤振信息
                                            if (sample.Id.Contains("_chatter_vibration"))
                                            {
                                                monitorData.ChatterVibration = sample.CDATA;
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

                                initDeviceMonitorData(monitorData);

                                if (!string.IsNullOrEmpty(deviceId))
                                {
                                    monitorData.DeviceId = deviceId;
                                    monitorData.Address = connection.Address;
                                    monitorData.Port = connection.Port;

                                    //从数据库检索Agent代理信息
                                    var agent = Database.ReadAgent(deviceId);
                                    if (agent != null)
                                    {
                                        monitorData.AgentInstanceId = agent.InstanceId;
                                        monitorData.Timestamp = agent.Timestamp;
                                    }

                                    //检索Device信息，此处检索name和uuid
                                    var device = Database.ReadDevice(deviceId, monitorData.AgentInstanceId);
                                    if (device != null)
                                    {
                                        monitorData.Name = device.Name;
                                        monitorData.Uuid = device.Uuid;
                                    }

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

                                                //根据约定的数据的id命名规则来判断数据项具体属于DeviceMonitorData的哪一个成员
                                                //急停
                                                if (sample.Id.Contains("_estop"))
                                                {
                                                    monitorData.EStop = sample.CDATA;
                                                }
                                                

                                                //控制器模式
                                                if (sample.Id.Contains("_controller_mode"))
                                                {
                                                    monitorData.ControllerMode = sample.CDATA;
                                                }
                                                

                                                //系统报警状态Fault、Warning等
                                                if (sample.Id.Contains("_system_status"))
                                                {
                                                    monitorData.SystemStatus = sample.CDATA;
                                                }

                                                //系统报警具体信息
                                                if (sample.Id.Contains("_system_message"))
                                                {
                                                    monitorData.SystemMessage = sample.CDATA;
                                                }

                                                //程序执行状态
                                                if (sample.Id.Contains("_execution"))
                                                {
                                                    monitorData.Execution = sample.CDATA;
                                                }
                                                

                                                //程序名
                                                if (sample.Id.Contains("_program"))
                                                {
                                                    monitorData.Execution = sample.CDATA;
                                                }
                                                

                                                //主轴转速
                                                if (sample.Id.Contains("_spindle_rotary_velocity"))
                                                {
                                                    monitorData.SpindleRotaryVelocity = sample.CDATA;
                                                }
                                                

                                                //主轴负载
                                                if (sample.Id.Contains("_spindle_load"))
                                                {
                                                    monitorData.SpindleLoad = sample.CDATA;
                                                }
                                                

                                                //进给速度
                                                if (sample.Id.Contains("_path_feedrate"))
                                                {
                                                    monitorData.PathFeedrate = sample.CDATA;
                                                }
                                                

                                                //XYZABC进给轴位置
                                                if (sample.Id.Contains("_xpos"))
                                                {
                                                    monitorData.XPos = sample.CDATA;
                                                }
                                                

                                                if (sample.Id.Contains("_ypos"))
                                                {
                                                    monitorData.YPos = sample.CDATA;
                                                }
                                                

                                                if (sample.Id.Contains("_zpos"))
                                                {
                                                    monitorData.ZPos = sample.CDATA;
                                                }
                                                

                                                if (sample.Id.Contains("_aangle"))
                                                {
                                                    monitorData.AAngle = sample.CDATA;
                                                }
                                                

                                                if (sample.Id.Contains("_bangle"))
                                                {
                                                    monitorData.BAngle = sample.CDATA;
                                                }
                                                

                                                if (sample.Id.Contains("_cangle"))
                                                {
                                                    monitorData.CAngle = sample.CDATA;
                                                }
                                                

                                                //XYZABC进给轴速度
                                                if (sample.Id.Contains("_xvelocity"))
                                                {
                                                    monitorData.XAxisFeedrate = sample.CDATA;
                                                }
                                                

                                                if (sample.Id.Contains("_yvelocity"))
                                                {
                                                    monitorData.YAxisFeedrate = sample.CDATA;
                                                }
                                                

                                                if (sample.Id.Contains("_zvelocity"))
                                                {
                                                    monitorData.ZAxisFeedrate = sample.CDATA;
                                                }
                                                

                                                if (sample.Id.Contains("_arotary_velocity"))
                                                {
                                                    monitorData.ARotaryVelocity = sample.CDATA;
                                                }
                                                

                                                if (sample.Id.Contains("_brotary_velocity"))
                                                {
                                                    monitorData.BRotaryVelocity = sample.CDATA;
                                                }
                                                

                                                if (sample.Id.Contains("_crotary_velocity"))
                                                {
                                                    monitorData.CRotaryVelocity = sample.CDATA;
                                                }
                                                

                                                //XYZABC进给轴负载
                                                if (sample.Id.Contains("_xload"))
                                                {
                                                    monitorData.XLoad = sample.CDATA;
                                                }
                                                

                                                if (sample.Id.Contains("_yload"))
                                                {
                                                    monitorData.YLoad = sample.CDATA;
                                                }
                                                

                                                if (sample.Id.Contains("_zload"))
                                                {
                                                    monitorData.ZLoad = sample.CDATA;
                                                }
                                                

                                                if (sample.Id.Contains("_aload"))
                                                {
                                                    monitorData.ALoad = sample.CDATA;
                                                }
                                                

                                                if (sample.Id.Contains("_bload"))
                                                {
                                                    monitorData.BLoad = sample.CDATA;
                                                }
                                                

                                                if (sample.Id.Contains("_cload"))
                                                {
                                                    monitorData.CLoad = sample.CDATA;
                                                }
                                                

                                                //传感器功率、能耗
                                                if (sample.Id.Contains("_power"))
                                                {
                                                    monitorData.Power = sample.CDATA;
                                                }
                                                
                                                if (sample.Id.Contains("_electrical_energy"))
                                                {
                                                    monitorData.ElectricalEnergy = sample.CDATA;
                                                }
                                                

                                                //传感器监测的机床颤振信息
                                                if (sample.Id.Contains("_chatter_vibration"))
                                                {
                                                    monitorData.ChatterVibration = sample.CDATA;
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

        private void initDeviceMonitorData(DeviceMonitorData monitorData)
        {
            monitorData.DeviceId = "";
            monitorData.Name = "";
            monitorData.Uuid = "";
            monitorData.AgentInstanceId = 123;
            monitorData.Address = "";
            monitorData.Port = 80;
            monitorData.Timestamp = DateTime.Now;
            monitorData.Available = false;
            monitorData.Connected = false;
            monitorData.EStop = "";
            monitorData.ControllerMode = "";
            monitorData.SystemStatus = "";
            monitorData.SystemMessage = "";
            monitorData.Execution = "";
            monitorData.Program = "";
            monitorData.SpindleRotaryVelocity = "";
            monitorData.SpindleLoad = "";
            monitorData.PathFeedrate = "";
            monitorData.XPos = "";
            monitorData.YPos = "";
            monitorData.ZPos = "";
            monitorData.AAngle = "";
            monitorData.BAngle = "";
            monitorData.CAngle = "";
            monitorData.XAxisFeedrate = "";
            monitorData.YAxisFeedrate = "";
            monitorData.ZAxisFeedrate = "";
            monitorData.ARotaryVelocity = "";
            monitorData.BRotaryVelocity = "";
            monitorData.CRotaryVelocity = "";
            monitorData.XLoad = "";
            monitorData.YLoad = "";
            monitorData.ZLoad = "";
            monitorData.ALoad = "";
            monitorData.BLoad = "";
            monitorData.CLoad = "";
            monitorData.Power = "";
            monitorData.ElectricalEnergy = "";
            monitorData.ChatterVibration = "";
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
