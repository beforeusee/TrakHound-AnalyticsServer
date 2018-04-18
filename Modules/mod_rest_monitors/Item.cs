﻿using System;
using Newtonsoft.Json;

namespace mod_rest_monitors
{
    class Item
    {
        [JsonProperty("device_id")]
        public string DeviceId { get; set; }

        [JsonProperty("agent_instance_id")]
        public long AgentInstanceId { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("port")]
        public long Port { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("available")]
        public bool Available { get; set; }

        [JsonProperty("connected")]
        public bool Connected { get; set; }

        [JsonProperty("estop")]
        public string EStop { get; set; }

        [JsonProperty("controller_mode")]
        public string ControllerMode { get; set; }

        [JsonProperty("system")]
        public string System { get; set; }

        [JsonProperty("execution")]
        public string Execution { get; set; }

        [JsonProperty("program")]
        public string Program { get; set; }

        [JsonProperty("spindle_velocity")]
        public string SpindleVelocity { get; set; }

        [JsonProperty("spindle_load")]
        public string SpindleLoad { get; set; }

        [JsonProperty("feedrate")]
        public string Feedrate { get; set; }

        [JsonProperty("xpos")]
        public string XPos{ get; set; }

        [JsonProperty("ypos")]
        public string YPos { get; set; }

        [JsonProperty("zpos")]
        public string ZPos { get; set; }

        [JsonProperty("aangle")]
        public string AAngle { get; set; }

        [JsonProperty("bangle")]
        public string BAngle { get; set; }

        [JsonProperty("cangle")]
        public string CAngle { get; set; }

        [JsonProperty("xvelocity")]
        public string XVelocity { get; set; }

        [JsonProperty("yvelocity")]
        public string YVelocity { get; set; }

        [JsonProperty("zvelocity")]
        public string ZVelocity { get; set; }

        [JsonProperty("arotary_velocity")]
        public string ARotaryVelocity { get; set; }

        [JsonProperty("brotary_velocity")]
        public string BRotaryVelocity { get; set; }

        [JsonProperty("crotary_velocity")]
        public string CRotaryVelocity { get; set; }

        [JsonProperty("xload")]
        public string XLoad { get; set; }

        [JsonProperty("yload")]
        public string YLoad { get; set; }

        [JsonProperty("zload")]
        public string ZLoad { get; set; }

        [JsonProperty("aload")]
        public string ALoad { get; set; }

        [JsonProperty("bload")]
        public string BLoad { get; set; }

        [JsonProperty("cload")]
        public string CLoad { get; set; }

        [JsonProperty("power")]
        public string Power { get; set; }

        [JsonProperty("energy_consumption")]
        public string EnergyConsumption { get; set; }

        [JsonProperty("chatter_vibration")]
        public string ChatterVibration { get; set; }

        public Item(DeviceMonitorData deviceMonitorData)
        {
            DeviceId = deviceMonitorData.DeviceId;
            AgentInstanceId = deviceMonitorData.AgentInstanceId;
            Address = deviceMonitorData.Address;
            Port = deviceMonitorData.Port;
            Timestamp = deviceMonitorData.Timestamp;
            Available = deviceMonitorData.Available;
            Connected = deviceMonitorData.Connected;
            EStop = deviceMonitorData.EStop;
            ControllerMode = deviceMonitorData.ControllerMode;
            System = deviceMonitorData.System;
            Execution = deviceMonitorData.Execution;
            Program = deviceMonitorData.Program;
            SpindleVelocity = deviceMonitorData.SpindleVelocity;
            SpindleLoad = deviceMonitorData.SpindleLoad;
            Feedrate = deviceMonitorData.Feedrate;
            XPos = deviceMonitorData.XPos;
            YPos = deviceMonitorData.YPos;
            ZPos = deviceMonitorData.ZPos;
            AAngle = deviceMonitorData.AAngle;
            BAngle = deviceMonitorData.BAngle;
            CAngle = deviceMonitorData.CAngle;
            XVelocity = deviceMonitorData.XVelocity;
            YVelocity = deviceMonitorData.YVelocity;
            ZVelocity = deviceMonitorData.ZVelocity;
            ARotaryVelocity = deviceMonitorData.ARotaryVelocity;
            BRotaryVelocity = deviceMonitorData.BRotaryVelocity;
            CRotaryVelocity = deviceMonitorData.CRotaryVelocity;
            XLoad = deviceMonitorData.XLoad;
            YLoad = deviceMonitorData.YLoad;
            ZLoad = deviceMonitorData.ZLoad;
            ALoad = deviceMonitorData.ALoad;
            BLoad = deviceMonitorData.BLoad;
            CLoad = deviceMonitorData.CLoad;
            Power = deviceMonitorData.Power;
            EnergyConsumption = deviceMonitorData.EnergyConsumption;
            ChatterVibration = deviceMonitorData.ChatterVibration;
        }
    }
}
