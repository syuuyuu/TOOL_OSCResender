using System;
//以下を追加した
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Threading;


//using System.Text.RegularExpressions;

namespace OSCResender {
    class UDPListener {

        static DispatcherObject UI;
        public static ObservableCollection<string> RecievedList { get; set; } = new ObservableCollection<string>();

        public static void Init(DispatcherObject ui) {
            UI = ui;
            if (InitPort()) {
                InitThread();
            }
        }
        static private UdpClient s_udpClient;
        static private string _logText = string.Empty;

        static Thread RecvThread = null;
        static private void InitThread() {
            RecvThread = new Thread(RecvUDP);
            RecvThread.Start();
        }

        static public void ChangePort(int port) {
            s_udpClient.Close();
            s_udpClient = new UdpClient(port);
            s_udpClient.Client.ReceiveTimeout = 10; // msec
            s_udpClient.Client.Blocking = false;
        }

        static List<String> IPList = new List<string>() { "", "", "" };
        static List<int> PortList = new List<int>() { -1, -1, -1 };

        static public void ChangeSendDist(string ip, int port, int id) {
            IPList[id - 1] = ip;
            PortList[id - 1] = port;
        }

        static private bool InitPort() {
            var ret = false;
            try {
                s_udpClient = new UdpClient(11111);
                s_udpClient.Client.ReceiveTimeout = 1; // msec
                s_udpClient.Client.Blocking = false;
                ret = true;
            } catch (Exception) {
                //
            }
            return ret;
        }

        internal static void ClearSendDist(int id) {
            IPList[id - 1] = null;
            PortList[id - 1] = -1;
        }

        static public string LogText {
            get { return _logText; }
            set {
                _logText = value;
            }
        }
        public delegate void AsyncMethodCaller(byte[] udpdata, int counti);
        const int MAC_LENGTH = 4 * 2;
        const int DATA_LENGTH = 14 * 2;

        public delegate void UpdateDelegate(string str);
        static uint MessageCount = 0;
        static public void RecvUDP() {
            StringBuilder sb = new StringBuilder();
            IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);

            while (true) {
                //while (true) {
                sb.Clear();
                try {
                    byte[] udpdata = s_udpClient.Receive(ref anyIP);               
                    sb.Append(Encoding.ASCII.GetString(udpdata));
                    if (sb.Length != 0) {
                        Console.WriteLine(sb.ToString());

                        for (int counti = 0; counti < IPList.Count; counti++) {
                            if (IPList[counti] != null && PortList[counti] > 0) {
                                AsyncMethodCaller caller = new AsyncMethodCaller(doSend);
                                caller.BeginInvoke(udpdata, counti, null, null);
                                //                                doSend(udpdata, counti);
                            }
                        }
                        sb.Insert(0, "\n");
                        sb.Insert(0, MessageCount.ToString());
                        MessageCount++;
                        UI.Dispatcher.BeginInvoke(DispatcherPriority.Background, new UpdateDelegate(UpdateList), sb.ToString());
                    }
                } catch (Exception) {               
//                    break;
                    // do nothing 
                }
                //}
                Thread.Sleep(10);
            }
        }

        private static void UpdateList(string str) {
            RecievedList.Insert(0, str);
            if (RecievedList.Count > 20) {
                RecievedList.RemoveAt(RecievedList.Count - 1);
            }
        }

        private static void doSend(byte[] udpdata, int counti) {
            try {
                s_udpClient.Send(udpdata, udpdata.Length, IPList[counti], PortList[counti]);
            } catch {

            }
        }

        private static void RecordPacket(string str) {
            Console.WriteLine(str);
        }

        internal static void ReleasePort() {
            Thread.Sleep(200);
            try {
                //s_udpClient.Dispose();
                s_udpClient.Close();
            } catch (Exception ex) {
                ex.ToString();
            }

            try {
                RecvThread.Abort();
            } catch (Exception ex) {
                ex.ToString();
            }

            Thread.Sleep(300);
        }
    }
}
