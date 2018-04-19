using System;
using Newtonsoft.Json;

namespace mod_rest_monitors
{
    class DeviceMonitorData
    {
        [JsonProperty("device_id")]
        public string DeviceId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("uuid")]
        public string Uuid { get; set; }

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

        [JsonProperty("system_status")]
        public string SystemStatus { get; set; }

        [JsonProperty("system_message")]
        public string SystemMessage { get; set; }

        [JsonProperty("execution")]
        public string Execution { get; set; }

        [JsonProperty("program")]
        public string Program { get; set; }

        [JsonProperty("spindle_rotary_velocity")]
        public string SpindleRotaryVelocity { get; set; }

        [JsonProperty("spindle_load")]
        public string SpindleLoad { get; set; }

        [JsonProperty("path_feedrate")]
        public string PathFeedrate { get; set; }

        [JsonProperty("xpos")]
        public string XPos { get; set; }

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

        [JsonProperty("xaxis_feedrate")]
        public string XAxisFeedrate { get; set; }

        [JsonProperty("yaxis_feedrate")]
        public string YAxisFeedrate { get; set; }

        [JsonProperty("zaxis_feedrate")]
        public string ZAxisFeedrate { get; set; }

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

        [JsonProperty("electrical_energy")]
        public string ElectricalEnergy { get; set; }

        [JsonProperty("chatter_vibration")]
        public string ChatterVibration { get; set; }

    }
}
