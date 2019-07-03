using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

namespace Client
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string ipServer;
        private int port;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                dialog.InitialDirectory = Environment.SpecialFolder.Personal.ToString();
                dialog.IsFolderPicker = true;
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    pathTextBox.Text = dialog.FileName;
                }
            }
            catch (InvalidOperationException exception)
            {
                MessageBox.Show(exception.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            ipServer = ipTextBox.Text;
            port = int.Parse(portTextBox.Text);

            TimerCallback tm = new TimerCallback(GetScreenFromServer);

            int period = 0;
            if (int.TryParse(periodTextBox.Text, out period) && period > 0)
            {
                Timer timer = new Timer(tm, client, 0, period * 1000);
            }
            else
                MessageBox.Show("Проверьте правильность введенных данных", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void GetScreenFromServer(object state)
        {
            Socket client = state as Socket;
            try
            {
                EndPoint srvEP = null;
                string path = "";
                Dispatcher.Invoke(() =>
                {
                    srvEP = new IPEndPoint(IPAddress.Parse(ipServer), port);

                    string FileName = "desktop_image_" + (new Random().Next(int.MaxValue) + ".jpeg");
                    path = pathTextBox.Text + "\\" + FileName;
                });

                string str = "Скриншот";
                client.SendTo(Encoding.UTF8.GetBytes(str), srvEP);

                byte[] buffer = new byte[1500];

                // получить количество посылок
                client.ReceiveFrom(buffer, ref srvEP);
                int cntRecive = BitConverter.ToInt32(buffer, 0);

                using (var stream = new FileStream(path, FileMode.OpenOrCreate))
                {
                    for (int i = 0; i < cntRecive; i++)
                    {
                        int recSize = client.ReceiveFrom(buffer, ref srvEP);
                        if (recSize > 0)
                        {
                            stream.Write(buffer, 0, recSize);
                        }
                        else
                            break;
                    }
                }
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
