﻿using OsuDataDistributeRestful.Api;
using OsuDataDistributeRestful.Server;
using Sync.Plugins;
using Sync.Tools;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace OsuDataDistributeRestful
{
    [SyncSoftRequirePlugin("OsuRTDataProviderPlugin", "RealTimePPDisplayerPlugin", "OsuLiveStatusPanelPlugin")]
    [SyncPluginID("50549ae4-8ba8-4b3b-9d18-9828d43c6523", VERSION)]
    public class OsuDataDistributeRestfulPlugin : Plugin
    {
        public const string PLUGIN_NAME = "OsuDataDistributeRestful";
        public const string PLUGIN_AUTHOR = "KedamavOvO";
        public const string VERSION = "0.3.1";

        public ApiServer ApiServer { get; private set; }
        private FileServer fileHttpServer;

        private PluginConfigurationManager m_config_manager;

        public OsuDataDistributeRestfulPlugin() : base(PLUGIN_NAME, PLUGIN_AUTHOR)
        {
            I18n.Instance.ApplyLanguage(new DefaultLanguage());
            m_config_manager = new PluginConfigurationManager(this);
            m_config_manager.AddItem(new SettingIni());

            ApiServer = new ApiServer(Setting.ApiPort);
            EventBus.BindEvent<PluginEvents.ProgramReadyEvent>(e => ApiServer.Start());
            if (Setting.AllowLAN)
                EventBus.BindEvent<PluginEvents.ProgramReadyEvent>(e => PrintLanAddress());

            if (Setting.EnableFileHttpServer)
            {
                fileHttpServer = new FileServer(Setting.FilePort);
                fileHttpServer.Start();
            }
        }

        private void PrintLanAddress()
        {
            //Display IP Address
            var ips = Dns.GetHostEntry(Dns.GetHostName()).AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).Distinct();
            int n = 1;
            foreach (var ip in ips)
            {
                bool recommend = ip.ToString().StartsWith("192.168.");
                IO.CurrentIO.WriteColor($"[ODDR]IP {n++}:{ip}", recommend ? ConsoleColor.Green : ConsoleColor.White);
            }
        }

        #region Initializtion

        private void ORTDP_Initialize()
        {
            var plugin = getHoster().EnumPluings().Where(p => p.Name == "OsuRTDataProvider").FirstOrDefault();
            if (plugin != null)
            {
                ApiServer.RegisterResource(new OrtdpApis(plugin));
            }
            else
                IO.CurrentIO.WriteColor($"[ODDR]Not Found OsuRTDataProvider", ConsoleColor.Red);
        }

        private void RTPPD_Initialize()
        {
            var plugin = getHoster().EnumPluings().Where(p => p.Name == "RealTimePPDisplayer").FirstOrDefault();
            if (plugin != null)
            {
                ApiServer.RegisterResource(new RtppdApis(plugin));
            }
            else
                IO.CurrentIO.WriteColor($"[ODDR]Not Found RealTimePPDisplayer", ConsoleColor.Red);
        }

        private void OLSP_Initialize()
        {
            var plugin = getHoster().EnumPluings().Where(p => p.Name == "OsuLiveStatusPanelPlugin").FirstOrDefault();
            if (plugin != null)
            {
                ApiServer.RegisterResource(new OlspApis(plugin));
            }
            else
                IO.CurrentIO.WriteColor($"[ODDR]Not Found OsuLiveStatusPanel", ConsoleColor.Red);
        }

        #endregion Initializtion

        private void Initialize()
        {
            ORTDP_Initialize();
            RTPPD_Initialize();
            OLSP_Initialize();
        }

        public override void OnEnable()
        {
            Sync.Tools.IO.CurrentIO.WriteColor(PLUGIN_NAME + " By " + PLUGIN_AUTHOR, ConsoleColor.DarkCyan);
            Initialize();
        }

        public override void OnExit()
        {
            if (Setting.EnableFileHttpServer)
                fileHttpServer?.Stop();
        }
    }
}