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
                // --- ĐOẠN MỚI: LẤY PORT TỪ GIAO DIỆN ---
                // Parse số từ TextBox, nếu lỗi thì mặc định về 8080
                if (!int.TryParse(txtPort.Text, out int port) || port < 1 || port > 65535)
                {
                    Log("[-] Invalid Port! Please enter 1 - 65535.");
                    return;
                }

                listener = new TcpListener(IPAddress.Any, port); // Dùng biến port vừa lấy
                listener.Start();
                // ---------------------------------------

                running = true;

                // Update giao diện
                btnStart.Text = "STOP";
                btnStart.BackColor = System.Drawing.Color.DarkRed; // Đổi màu nút thành đỏ cho ngầu
                txtPort.Enabled = false; // Khóa ô nhập port lại

                this.Text = $"Server Monitor - [ONLINE] - Port {port}";
                Log($"[*] Server started. Listening on Port {port}...");

                // Thread chờ kết nối client
                Thread t = new Thread(AcceptLoop);
                t.IsBackground = true;
                t.Start();
            }
            catch (Exception ex)
            {
                Log("[-] Error starting server: " + ex.Message);
                txtPort.Enabled = true; // Lỗi thì mở lại cho nhập
            }
        }

        void StopServer()
        {
            running = false;
            listener?.Stop();

            // Reset giao diện
            btnStart.Text = "START";
            btnStart.BackColor = System.Drawing.Color.FromArgb(0, 122, 204); // Trả về màu xanh
            txtPort.Enabled = true; // Mở lại cho sửa Port

            this.Text = "Server Monitor - [OFFLINE]";
            Log("[-] Server stopped.");
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
                // Lấy IP client gửi file
                IPEndPoint remote = (IPEndPoint)client.Client.RemoteEndPoint;
                string clientIP = remote.Address.ToString();

                // Nhận độ dài tên file 
                byte[] len = new byte[4];
                stream.Read(len, 0, 4);
                int nameLen = BitConverter.ToInt32(len, 0);

                // Nhận tên file 
                byte[] nameData = new byte[nameLen];
                stream.Read(nameData, 0, nameLen);
                string fileName = Encoding.UTF8.GetString(nameData);

                // Nhận kích thước file 
                byte[] sizeData = new byte[8];
                stream.Read(sizeData, 0, 8);
                long fileSize = BitConverter.ToInt64(sizeData, 0);

                // Log thêm IP người gửi
                Log($"Client {clientIP} đang gửi file {fileName} ({FormatBytes(fileSize)})");

                // Chuẩn bị lưu file
                string folder = Path.Combine(Application.StartupPath, "Received");
                Directory.CreateDirectory(folder);

                string savePath = Path.Combine(folder, DateTime.Now.ToString("HHmmss") + "_" + fileName);

                // Nhận dữ liệu
                using (FileStream fs = new FileStream(savePath, FileMode.Create))
                {
                    byte[] buffer = new byte[1024 * 1024];
                    long received = 0;
                    int read;   

                    Log($"Đang nhận file từ {clientIP} ...");

                    while (received < fileSize && (read = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fs.Write(buffer, 0, read);
                        received += read;
                    }
                }

                Log($"NHẬN THÀNH CÔNG từ {clientIP}: {fileName}");
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

        // Sự kiện Click cho nút Open Folder
        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            try
            {
                // Đường dẫn thư mục Received
                string folderPath = Path.Combine(Application.StartupPath, "Received");

                // Nếu thư mục chưa tồn tại (chưa nhận file nào) thì tạo mới luôn
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Mở thư mục bằng Windows Explorer
                System.Diagnostics.Process.Start("explorer.exe", folderPath);

                Log("[*] System: Opened 'Received' folder.");
            }
            catch (Exception ex)
            {
                Log("[-] Error opening folder: " + ex.Message);
            }
        }
    }
}
