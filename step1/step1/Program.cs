using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace ScreenSharingServer
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ScreenSharingForm());
        }
    }

    public class ScreenSharingForm : Form
    {
        private PictureBox pictureBox;
        private TextBox outputTextBox;
        private UdpClient udpClient;

        public ScreenSharingForm()
        {
            pictureBox = new PictureBox();
            outputTextBox = new TextBox();
            InitializeComponents();
            StartServer();
        }

        private void InitializeComponents()
        {
            pictureBox = new PictureBox();
            pictureBox.Dock = DockStyle.Fill;
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;

            outputTextBox.Dock = DockStyle.Bottom;
            outputTextBox.Multiline = true;
            outputTextBox.ScrollBars = ScrollBars.Vertical;
            outputTextBox.ReadOnly = true;

            this.Controls.Add(pictureBox);
            this.Controls.Add(outputTextBox);
        }

        private void StartServer()
        {
            try
            {
                int port = 8080;
                udpClient = new UdpClient(port);

                WriteLine("Waiting for connections...");

                Thread receiveThread = new Thread(new ThreadStart(ReceiveData));
                receiveThread.IsBackground = true;
                receiveThread.Start();
            }
            catch (Exception ex)
            {
                WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private void ReceiveData()
        {
            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
                MemoryStream imageStream = new MemoryStream();

                while (true)
                {
                    byte[] chunkData = udpClient.Receive(ref endPoint);
                    imageStream.Write(chunkData, 0, chunkData.Length);

                    // Check if this is the last chunk
                    if (chunkData.Length < 1024) // Assuming chunk size is less than 1024
                    {
                        // Display received image
                        DisplayReceivedImage(imageStream.ToArray());

                        // Reset image stream for the next transmission
                        imageStream = new MemoryStream();
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private void DisplayReceivedImage(byte[] imageData)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream(imageData))
                {
                    Image image = Image.FromStream(memoryStream);
                    pictureBox.Image = image;
                }
            }
            catch (Exception ex)
            {
                WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private void WriteLine(string text)
        {
            if (outputTextBox.InvokeRequired)
            {
                outputTextBox.Invoke(new Action<string>(WriteLine), text);
            }
            else
            {
                outputTextBox.AppendText(text + Environment.NewLine);
            }
        }
    }
}
