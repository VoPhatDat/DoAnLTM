using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace FileTransferServer
{
    public partial class Form1 : Form
    {
        TcpListener listener;      // Lắng nghe kết nối TCP từ client
        bool running = false;      // Trạng thái server

        public Form1()
        {
            InitializeComponent();
            this.Text = "File Transfer Server - Chưa chạy";
            btnStart.Text = "Start";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (running) StopServer();
            else StartServer();
        }


        void StartServer()
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, 8080); // Nhận trên mọi IP máy
                listener.Start();

                running = true;
                btnStart.Text = "Stop";
                this.Text = "File Transfer Server - Port 8080 (Đang chạy)";
                Log("Server đã khởi động! Đang chờ client...");

                // Thread chờ kết nối client
                Thread t = new Thread(AcceptLoop);
                t.IsBackground = true;
                t.Start();
            }
            catch (Exception ex)
            {
                Log("Lỗi: " + ex.Message);
            }
        }

  
        void StopServer()
        {
            running = false;
            listener?.Stop();

            btnStart.Text = "Start";
            this.Text = "File Transfer Server - Đã dừng";
            Log("Server đã dừng.");
        }


        void AcceptLoop()
        {
            while (running)
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient(); // Chấp nhận client mới
                    Log($"Client kết nối từ: {((IPEndPoint)client.Client.RemoteEndPoint).Address}");

                    // Tạo thread xử lý từng client riêng
                    Thread clientThread = new Thread(HandleClient);
                    clientThread.IsBackground = true;
                    clientThread.Start(client);
                }
                catch { }
            }
        }

        void HandleClient(object obj)
        {
            TcpClient client = (TcpClient)obj;
            NetworkStream stream = client.GetStream();

            try
            {
                // Nhận độ dài tên file 
                byte[] len = new byte[4];
                stream.Read(len, 0, 4);
                int nameLen = BitConverter.ToInt32(len, 0);

                // Nhận tên file 
                byte[] nameData = new byte[nameLen];
                stream.Read(nameData, 0, nameLen);
                string fileName = Encoding.UTF8.GetString(nameData);

                // Nhận kích thước file (8 bytes - long) 
                byte[] sizeData = new byte[8];
                stream.Read(sizeData, 0, 8);
                long fileSize = BitConverter.ToInt64(sizeData, 0);

                // Chuẩn bị thư mục lưu file 
                string folder = Path.Combine(Application.StartupPath, "Received");
                Directory.CreateDirectory(folder);

                string savePath = Path.Combine(folder,
                    DateTime.Now.ToString("HHmmss") + "_" + fileName);

                // Đọc dữ liệu file qua stream (CLO2.2 + CLO3.1) 
                using (FileStream fs = new FileStream(savePath, FileMode.Create))
                {
                    byte[] buffer = new byte[1024 * 1024]; // 1MB buffer
                    long received = 0;
                    int read;

                    Log($"Đang nhận: {fileName} ({FormatBytes(fileSize)})");

                    // Đọc luồng TCP và ghi vào FileStream
                    while (received < fileSize && (read = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fs.Write(buffer, 0, read);
                        received += read;
                    }
                }

                Log($"NHẬN THÀNH CÔNG: {fileName} (lưu trong thư mục Received/)");
            }
            catch (Exception ex)
            {
                Log("Lỗi nhận file: " + ex.Message);
            }
            finally
            {
                client.Close();
            }
        }

        // Ghi log lên RichTextBox
        void Log(string s)
        {
            if (richTextBox1.InvokeRequired)
            {
                Invoke(new Action(() => Log(s)));
                return;
            }

            richTextBox1.AppendText($"[{DateTime.Now:HH:mm:ss}] {s}\n");
            richTextBox1.ScrollToCaret();
        }

        // Format dung lượng
        string FormatBytes(long b)
        {
            string[] u = { "B", "KB", "MB", "GB" };
            double s = b;
            int i = 0;

            while (s >= 1024 && i < u.Length - 1)
            {
                s /= 1024;
                i++;
            }

            return $"{s:F1} {u[i]}";
        }

        // Đảm bảo server tắt khi đóng form
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            StopServer();
            base.OnFormClosing(e);
        }

        private void Form1_Load(object sender, EventArgs e) { }
    }
}
