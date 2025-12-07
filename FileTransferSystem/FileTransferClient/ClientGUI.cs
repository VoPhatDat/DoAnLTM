using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace FileTransferClient
{
    public partial class ClientGUI : Form
    {
        private string filePath = "";     // Đường dẫn file được chọn
        private long fileSize = 0;        // Dung lượng file
        private long sentBytes = 0;       // Số byte đã gửi
        private DateTime startTime;       // Thời điểm bắt đầu gửi file (tính tốc độ)

        public ClientGUI()
        {
            InitializeComponent();

            // Giá trị mặc định để test nhanh
            txtServerIP.Text = "127.0.0.1";
            txtPort.Text = "8080";
            label5.Text = "Đã gửi: 0 / 0";
            label6.Text = "Tốc độ: 0 MB/s";

            // Gán sự kiện cho nút
            btnSend.Click += btnSend_Click;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Tất cả file (*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    filePath = ofd.FileName;
                    txtFilePath.Text = filePath;

                    fileSize = new FileInfo(filePath).Length;
                    label5.Text = $"Đã gửi: 0 / {FormatBytes(fileSize)}";
                    progressBar1.Value = 0;
                }
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            // Kiểm tra file tồn tại
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                MessageBox.Show("Chọn file trước đã!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnSend.Enabled = false;

            // Dùng thread riêng để tránh đứng giao diện
            Thread thread = new Thread(SendFile);
            thread.IsBackground = true;
            thread.Start();
        }

        private void SendFile()
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    // Kết nối tới server
                    client.Connect(txtServerIP.Text.Trim(), int.Parse(txtPort.Text.Trim()));

                    using (NetworkStream stream = client.GetStream())                    // Stream mạng (ghi)
                    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read)) // Stream file (đọc)
                    {
                        //  Gửi metadata (tên file)
                        string fileName = Path.GetFileName(filePath);
                        byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(fileName);      //strings sang byte
                        byte[] nameLen = BitConverter.GetBytes(nameBytes.Length);            //byte sang nhị phân để gửi

                        stream.Write(nameLen, 0, 4);                  // Gửi độ dài tên file
                        stream.Write(nameBytes, 0, nameBytes.Length); // Gửi tên file

                        //  Gửi kích thước file (8 byte - long)
                        byte[] sizeBytes = BitConverter.GetBytes(fileSize);
                        stream.Write(sizeBytes, 0, 8);

                        //  Đọc file theo luồng (FileStream) và gửi theo từng block (NetworkStream)
                  
                        byte[] buffer = new byte[1024 * 1024]; // Buffer 1MB
                        int bytesRead;
                        sentBytes = 0;
                        startTime = DateTime.Now;

                        while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0) // Đọc luồng file
                        {
                            stream.Write(buffer, 0, bytesRead); // Gửi luồng nhị phân 
                            sentBytes += bytesRead;

                            // Cập nhật UI (phải Invoke vì đang ở thread khác)
                            this.Invoke((Action)(() =>
                            {
                                progressBar1.Value = (int)((sentBytes * 100) / fileSize);
                                label5.Text = $"Đã gửi: {FormatBytes(sentBytes)} / {FormatBytes(fileSize)}";

                                double elapsed = (DateTime.Now - startTime).TotalSeconds;
                                double speed = elapsed > 0 ? (sentBytes / elapsed / 1048576.0) : 0;

                                label6.Text = $"Tốc độ: {speed:F2} MB/s";
                            }));
                        }
                    }
                }

                // Thông báo thành công
                this.Invoke((Action)(() =>
                {
                    MessageBox.Show("Gửi thành công 100%!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    btnSend.Enabled = true;
                }));
            }
            catch (Exception ex)
            {
                // Thông báo lỗi
                this.Invoke((Action)(() =>
                {
                    MessageBox.Show("Lỗi: " + ex.Message, "Thất bại", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btnSend.Enabled = true;
                }));
            }
        }

        // ----- Hàm format dung lượng cho dễ đọc -----
        private string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB" };
            double size = bytes;
            int i = 0;

            while (size >= 1024 && i < suffixes.Length - 1)
            {
                size /= 1024;
                i++;
            }

            return $"{size:F1} {suffixes[i]}";
        }

        private void btnSend_Click_1(object sender, EventArgs e)
        {
        }
    }
}
