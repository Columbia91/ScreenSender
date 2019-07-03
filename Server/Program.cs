using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, 12345));
            WaitForRequest(serverSocket);
        }
        static void WaitForRequest(Socket serverSocket)
        {
            try
            {
                byte[] buffer = new byte[1024];
                EndPoint clientEndPoint = new IPEndPoint(0, 0);

                int recSize = serverSocket.ReceiveFrom(buffer, ref clientEndPoint);
                string keyWord = "Скриншот";

                string path = "desktop_image_" + (new Random().Next(int.MaxValue) + ".jpeg"); ;
                Bitmap bmp = ImageFromScreen();
                bmp.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);

                if (keyWord == Encoding.UTF8.GetString(buffer, 0, recSize))
                {
                    using (var stream = new FileStream(path, FileMode.OpenOrCreate))
                    {
                        int packetSize = 1500;
                        long cntRecive = stream.Length / packetSize;
                        long remainder = stream.Length % packetSize;

                        if (remainder > 0) cntRecive++;
                        serverSocket.SendTo(BitConverter.GetBytes((int)cntRecive), clientEndPoint);

                        byte[] bmpBuffer = new byte[packetSize];

                        for (int i = 0; i < (remainder > 0 ? cntRecive - 1 : cntRecive); i++)
                        {
                            stream.Read(bmpBuffer, 0, packetSize);
                            serverSocket.SendTo(bmpBuffer, clientEndPoint);
                            Thread.Sleep(20);
                        }

                        if (remainder > 0)
                        {
                            stream.Read(bmpBuffer, 0, (int)remainder);
                            serverSocket.SendTo(bmpBuffer, 0, (int)remainder, SocketFlags.None, clientEndPoint);
                        }
                    }
                }
                WaitForRequest(serverSocket);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        static Bitmap ImageFromScreen()
        {
            Bitmap bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height,
                PixelFormat.Format32bppRgb);
            using (Graphics graphics = Graphics.FromImage(bmp))
            {
                graphics.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y,
                    0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);
            }
            return bmp;
        }
    }
}
