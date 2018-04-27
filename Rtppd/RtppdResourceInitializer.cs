﻿using OsuDataDistributeRestful;
using OsuDataDistributeRestful.Server;
using RealTimePPDisplayer;
using Sync.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuDataDistributeRestful.Rtppd
{
    class RtppdResourceInitializer
    {
        private RestfulDisplayer[] m_restfuile_displayers = new RestfulDisplayer[16];

        RestfulDisplayer GetDisplayer(int i)
        {
            if (i < 0 || i > 16) return null;

            var displayer = m_restfuile_displayers[i];
            return displayer;
        }

        public RtppdResourceInitializer(Plugin rtppd_plugin,ApiServer api)
        {
            var rtppd = rtppd_plugin as RealTimePPDisplayerPlugin;

            rtppd.RegisterDisplayer("restful", (id) => m_restfuile_displayers[id ?? 0] = new RestfulDisplayer(id));

            api.RegisterResource("/api/rtppd/{id}/playing", (p) => {
                RestfulDisplayer displayer = GetDisplayer(p.GetInt("id") ?? 0);
                return new
                {
                    client_id = displayer?.ClientID,
                    playing = displayer?.IsPlay
                };
            });

            api.RegisterResource("/api/rtppd/{id}/pp", (p) =>
            {
                RestfulDisplayer displayer = GetDisplayer(p.GetInt("id") ?? 0);

                return new
                {
                    client_id = displayer?.ClientID,
                    tuple = displayer?.PPTuple == null ? null : new
                    {
                        fcpp_aim = displayer?.PPTuple.FullComboAimPP,
                        fcpp_speed = displayer?.PPTuple.FullComboSpeedPP,
                        fcpp_acc = displayer?.PPTuple.FullComboAccuracyPP,
                        fcpp = displayer?.PPTuple.FullComboPP,

                        rtpp_aim = displayer?.PPTuple.RealTimeAimPP,
                        rtpp_speed = displayer?.PPTuple.RealTimeSpeedPP,
                        rtpp_acc = displayer?.PPTuple.RealTimeAccuracyPP,
                        rtpp = displayer?.PPTuple.RealTimePP,

                        maxpp_aim = displayer?.PPTuple.MaxAimPP,
                        maxpp_speed = displayer?.PPTuple.MaxSpeedPP,
                        maxpp_acc = displayer?.PPTuple.MaxAccuracyPP,
                        maxpp = displayer?.PPTuple.MaxPP,
                    }
                };
            });

            api.RegisterResource("/api/rtppd/{id}/hit_count", (p) =>
            {
                RestfulDisplayer displayer = GetDisplayer(p.GetInt("id") ?? 0);

                return new
                {
                    client_id=displayer?.ClientID,
                    tuple = displayer?.PPTuple == null ? null : new
                    {
                        n300g = displayer?.HitCountTuple.CountGeki,
                        n300 = displayer?.HitCountTuple.Count300,
                        n200 = displayer?.HitCountTuple.CountKatu,
                        n150 = displayer?.HitCountTuple.Count100,
                        n100 = displayer?.HitCountTuple.Count100,
                        n50 = displayer?.HitCountTuple.Count50,
                        nmiss = displayer?.HitCountTuple.CountMiss,

                        rtmaxcombo = displayer?.HitCountTuple.RealTimeMaxCombo,
                        maxcombo = displayer?.HitCountTuple.PlayerMaxCombo,
                        fullcombo = displayer?.HitCountTuple.FullCombo,
                        combo = displayer?.HitCountTuple.Combo
                    }
                };
            });

            api.RegisterResource("/api/rtppd/pp/format", (p) => new { format=StringFormatter.GetPPFormatter().Format });

            api.RegisterResource("/api/rtppd/hit_count/format", (p) => new { format=StringFormatter.GetHitCountFormatter().Format });

            api.RegisterResource("/api/rtppd/{id}/pp/formatted_content", (p) => {
                RestfulDisplayer displayer = GetDisplayer(p.GetInt("id") ?? 0);
                return new
                {
                    clietn_id=displayer?.ClientID,
                    content = displayer?.StringPP
                };
            });

            api.RegisterResource("/api/rtppd/{id}/hit_count/formatted_content", (p) => {
                RestfulDisplayer displayer = GetDisplayer(p.GetInt("id") ?? 0);
                return new
                {
                    client_id = displayer?.ClientID,
                    content = displayer?.StringHitCount
                };
            });
        }
    }
}
