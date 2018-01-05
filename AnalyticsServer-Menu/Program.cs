﻿// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using NLog;
using System;
using System.Security.Permissions;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using System.ServiceProcess;


namespace TrakHound.AnalyticsServer.Menu
{
    static class Program
    {
        private const int SERVICE_STATUS_INTERVAL = 1000;
        private const string SERVICE_NAME = "TrakHound-AnalyticsServer";

        private static Logger log = LogManager.GetCurrentClassLogger();
        private static SystemTrayMenu menu;
        private static ManualResetEvent stop;
        private static System.Timers.Timer serviceStatusTimer;

        internal static ServiceControllerStatus ServiceStatus;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            StartServiceStatusTimer();
            StartMenu();
        }

        private static void StartMenu()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.AddMessageFilter(new ReadMessageFilter());

            menu = new SystemTrayMenu();
            Application.Run(menu);
        }

        private static void StartServiceStatusTimer()
        {
            serviceStatusTimer = new System.Timers.Timer();
            serviceStatusTimer.Interval = SERVICE_STATUS_INTERVAL;
            serviceStatusTimer.Elapsed += ServiceStatusTimer_Elapsed;
            serviceStatusTimer.Start();
        }

        private static void ServiceStatusTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var sc = new ServiceController(SERVICE_NAME);
            if (sc != null)
            {
                var status = sc.Status;

                if (status != ServiceStatus)
                {
                    // Update Menu Status Label
                    SystemTrayMenu.SetHeader(status.ToString());

                    // Set NotifyIcon Icon
                    if (status == ServiceControllerStatus.Running) SystemTrayMenu.NotifyIcon.Icon = Properties.Resources.analyticsserver_status_running;
                    else SystemTrayMenu.NotifyIcon.Icon = Properties.Resources.analyticsserver_status_stopped;

                    // Create Notification
                    if (status == ServiceControllerStatus.Running || status == ServiceControllerStatus.Stopped)
                    {
                        var notifyIcon = SystemTrayMenu.NotifyIcon;
                        notifyIcon.BalloonTipTitle = "TrakHound AnalyticsServer";
                        notifyIcon.BalloonTipText = status.ToString();
                        notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                        notifyIcon.ShowBalloonTip(5000);
                    }
                }

                ServiceStatus = status;
            }
        }

        public static void Exit()
        {
            if (stop != null) stop.Set();
            if (menu != null) menu.Exit();
            if (serviceStatusTimer != null) serviceStatusTimer.Stop();
            Application.Exit();
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        private class ReadMessageFilter : IMessageFilter
        {
            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg == /*WM_CLOSE*/ 0x10)
                {
                    Exit();
                    return true;
                }

                return false;
            }
        }
    }
}