using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp.Server;
using OsuDataDistributeRestful.Server.Api;
using Sync.Tools;
using Sync.Plugins;
using RealTimePPDisplayer;
using OsuRTDataProvider;
using OsuRTDataProvider.Listen;
using Newtonsoft.Json;
using WebSocketSharp;

namespace OsuDataDistributeRestful
{
    // From https://dotblogs.com.tw/EganBlog/2019/05/25/WebSocket_Vue_WebSocketSharp
    public class MessageQueueSingletion
    {
        private static volatile MessageQueueSingletion _instance;
        private static object _lockObj = new object();
        private static ConcurrentQueue<string> _msgQueue;

        private MessageQueueSingletion() { }

        public static MessageQueueSingletion Instance()
        {
            if (_instance == null)
            {
                lock (_lockObj)
                {
                    if (_instance == null)
                    {
                        _instance = new MessageQueueSingletion();
                        _msgQueue = new ConcurrentQueue<string>();
                    }
                }
            }

            return _instance;
        }

        public void AddMessage(string msg)
        {
            try {
                _msgQueue.Enqueue(msg);
            } catch (Exception e) {
                IO.CurrentIO.WriteColor(String.Format(DefaultLanguage.Err, e.Message, e.ToString()), ConsoleColor.Red);
            }
        }

        public void GetMsg(out string msg)
        {
            msg = null;
            try
            {
                lock (_lockObj)
                {
                    if (!_msgQueue.TryDequeue(out msg))
                    {
                        msg = null;
                    }
                }
            } catch (Exception e)
            {
                IO.CurrentIO.WriteColor(String.Format(DefaultLanguage.Err, e.Message, e.ToString()), ConsoleColor.Red);
            }
        }

        public void FreeQueue()
        {
            lock (_lockObj)
            {
                string temp;
                while (_msgQueue.TryDequeue(out temp))
                {
                }
            }
        }
    }
    class WebSocket
    {
        public static WebSocketServer WebSocketServer { get; set; }
        protected OsuRTDataProviderPlugin _ortdp;
        protected RealTimePPDisplayerPlugin _rtppd;

        public void WebSocketServerStart()
        {
            IO.CurrentIO.Write("[WebSocket Server] Strating...");

            WebSocketServer = new WebSocketServer(Setting.WebSocketPort);
            WebSocketServer.AddWebSocketService<NotifyBehavior>("/");
            WebSocketServer.Start();

            IO.CurrentIO.Write("[WebSocket Server] Strated");
        }

        public void InitWebSocket(Plugin ortdp, Plugin rtppd)
        {
            _ortdp = ortdp as OsuRTDataProviderPlugin;
            _rtppd = rtppd as RealTimePPDisplayerPlugin;

            _ortdp.ListenerManager.OnStatusChanged += OnStatusChange;
            _ortdp.ListenerManager.OnAccuracyChanged += OnAccuracyChange;
            _ortdp.ListenerManager.OnComboChanged += OnComboChanged;

            _ortdp.ListenerManager.OnCountMissChanged += OnCountMissChanged;
            _ortdp.ListenerManager.OnCount50Changed += OnCount50Changed;
            _ortdp.ListenerManager.OnCount100Changed += OnCount100Changed;
            _ortdp.ListenerManager.OnCount300Changed += OnCount300Changed;
        }


        private void OnCount50Changed(int hit) { Hits(50, hit); }
        private void OnCount100Changed(int hit) { Hits(100, hit); }
        private void OnCount300Changed(int hit) { Hits(100, hit); }
        private void OnCountMissChanged(int hit) { Hits(0, hit); }

        private void Hits(int type, int hits)
        {
            MessageQueueSingletion.Instance().AddMessage(JsonConvert.SerializeObject(new
            {
                type = "hit",
                status = new
                {
                    type,
                    hits
                }
            }));
        }

        private void OnComboChanged(int combo)
        {
            MessageQueueSingletion.Instance().AddMessage(JsonConvert.SerializeObject(new
            {
                type = "combo",
                status = combo
            }));
        }

        private void OnStatusChange(OsuListenerManager.OsuStatus last_status, OsuListenerManager.OsuStatus status)
        {
            MessageQueueSingletion.Instance().AddMessage(JsonConvert.SerializeObject(new
            {
                type = "mode",
                status = last_status
            }));
        }

        private void OnAccuracyChange(double acc)
        {
            MessageQueueSingletion.Instance().AddMessage(JsonConvert.SerializeObject(new
            {
                type = "accuracy",
                status = acc
            }));
        }

        private class NotifyBehavior : WebSocketBehavior
        {
            public NotifyBehavior() { }

            protected override void OnOpen()
            {
                while (true)
                {
                    MessageQueueSingletion.Instance().GetMsg(out string msg);
                    if (msg != null) Sessions.BroadcastAsync(Encoding.UTF8.GetBytes(msg), null);
                    Thread.Sleep(1);
                }
            }

            protected override void OnMessage(MessageEventArgs e)
            {

            }

            protected override void OnClose(CloseEventArgs e)
            {
            
            }

            protected override void OnError(ErrorEventArgs e)
            {

            }
        }
    }
}
