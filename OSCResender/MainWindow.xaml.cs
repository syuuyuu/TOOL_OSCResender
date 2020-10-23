using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace OSCResender {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
    

        public MainWindow() {
            InitializeComponent();
            Button_Click(null, null);
            UDPListener.Init(this);
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            int port;
            port = -1;
            try {
                port = int.Parse(rp.Text);
                if (port > 0) {
                    UDPListener.ChangePort(port);
                }
            } catch (Exception) {

            }
            ChangeSendDist(i1, p1, 1);
            ChangeSendDist(i2, p2, 2);
            ChangeSendDist(i3, p3, 3);
        }

        private void ChangeSendDist(TextBox ii1, TextBox pp1, int vv) {
            int port = -1;
            try {
                port = int.Parse(pp1.Text);
                var ip = ii1.Text;
                if (ip != null && port > 0) {
                    UDPListener.ChangeSendDist(ip, port, vv);
                } else {
                    UDPListener.ClearSendDist(vv);
                }
            } catch (Exception) {

            }
        }

        private void Window_Closed(object sender, EventArgs e) {
            UDPListener.ReleasePort();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e) {
            UDPListener.RecievedList.Clear();
        }
    }
}
