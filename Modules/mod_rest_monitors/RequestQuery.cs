﻿using System;
using System.Web;
using Json = TrakHound.Api.v2.Json;

namespace mod_rest_monitors
{
    class RequestQuery
    {
        public string DeviceId { get; set; }

        public DateTime From { get; set; }

        public DateTime To { get; set; }

        public DateTime At { get; set; }

        public long Count { get; set; }

        public int Interval { get; set; }

        public string[] DataItems { get; set; }

        private bool _isValid = false;

        public bool IsValid { get { return _isValid; } }

        public RequestQuery(Uri uri)
        {

            if (uri != null)
            {
                var segments = uri.Segments;
                if (segments.Length > 1)
                {
                    string resource = segments[segments.Length - 1].ToLower().Trim('/');

                    //检查是否是请求的资源
                    if (resource == "monitor" || resource == "monitors")
                    {
                        //获取设备Id作为资源所有者
                        DeviceId = segments[segments.Length - 2].Trim('/');

                        if (!string.IsNullOrEmpty(DeviceId))
                        {
                            // From
                            string s = HttpUtility.ParseQueryString(uri.Query).Get("from");
                            DateTime from = DateTime.MinValue;
                            if (DateTime.TryParse(s, out from)) from = from.ToUniversalTime();
                            From = from;

                            // To
                            s = HttpUtility.ParseQueryString(uri.Query).Get("to");
                            DateTime to = DateTime.MinValue;
                            if (DateTime.TryParse(s, out to)) to = to.ToUniversalTime();
                            To = to;

                            //Count
                            s = HttpUtility.ParseQueryString(uri.Query).Get("count");
                            long count = 0;
                            long.TryParse(s, out count);
                            Count = count;

                            // At
                            s = HttpUtility.ParseQueryString(uri.Query).Get("at");
                            DateTime at = DateTime.MinValue;
                            if (DateTime.TryParse(s, out at)) at = at.ToUniversalTime();
                            At = at;

                            // Interval
                            s = HttpUtility.ParseQueryString(uri.Query).Get("interval");
                            int interval = 0;
                            int.TryParse(s, out interval);
                            Interval = interval;

                            // DataItems
                            s = HttpUtility.ParseQueryString(uri.Query).Get("data_items");
                            if (!string.IsNullOrEmpty(s))
                            {
                                DataItems = Json.Convert.FromJson<string[]>(s);
                            }
                        }
                        _isValid = true;
                    }
                }
            }
        }
    }
}
