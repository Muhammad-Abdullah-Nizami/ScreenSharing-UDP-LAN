using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ScreenSharingClient
{
    class Program
    {
        static void Main(string[] args)
        {
            StartClient();
        }

        static void StartClient()
        {
            try
            {
                string serverIpAddress = "192.168.1.2";
                int port = 8080;
                UdpClient udpClient = new UdpClient();

                while (true)
                {
                    Bitmap bitmap = new Bitmap(1366, 768);

                    using (Graphics graphics = Graphics.FromImage(bitmap))
                    {
                        graphics.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
                    }

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        bitmap.Save(memoryStream, ImageFormat.Jpeg);
                        byte[] imageData = memoryStream.ToArray();

                        int maxChunkSize = 1024; // Define maximum chunk size

                        for (int i = 0; i < imageData.Length; i += maxChunkSize)
                        {
                            int remainingLength = Math.Min(maxChunkSize, imageData.Length - i);
                            byte[] chunkData = new byte[remainingLength];
                            Array.Copy(imageData, i, chunkData, 0, remainingLength);

                            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(serverIpAddress), port);
                            udpClient.Send(chunkData, chunkData.Length, endPoint);
                        }
                    }

                    Console.WriteLine("Screen data sent to the server.");

                    // Delay 
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
