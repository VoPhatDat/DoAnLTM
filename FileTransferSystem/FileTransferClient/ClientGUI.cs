using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace FileTransferClient
{
    public partial class ClientGUI : Form
    {
        private string filePath = "";
        private long fileSize = 0;
        private long sentBytes = 0;
        private DateTime startTime;

        public ClientGUI()
        {
            InitializeComponent();

            // Giá trị mặc định
            txtServerIP.Text = "127.0.0.1";
            txtPort.Text = "8080";
            label5.Text = "Đã gửi: 0 / 0";
            label6.Text = "Tốc độ: 0 MB/s";

            // Gắn event
            btnSend.Click += btnSend_Click;
            btnBrowse.Click += btnBrowse_Click;
        }

        // Chọn file
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

        // Bấm gửi
        private void btnSend_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                MessageBox.Show("Chọn file trước đã!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnSend.Enabled = false;

            Thread thread = new Thread(SendFile);
            thread.IsBackground = true;
            thread.Start();
        }

        // Hàm gửi file
        private void SendFile()
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    client.Connect(txtServerIP.Text.Trim(), int.Parse(txtPort.Text.Trim()));

                    using (NetworkStream stream = client.GetStream())
                    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        // Gửi tên file
                        string fileName = Path.GetFileName(filePath);
                        byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(fileName);
                        byte[] nameLen = BitConverter.GetBytes(nameBytes.Length);

                        stream.Write(nameLen, 0, 4);
                        stream.Write(nameBytes, 0, nameBytes.Length);

                        // Gửi kích thước
                        byte[] sizeBytes = BitConverter.GetBytes(fileSize);
                        stream.Write(sizeBytes, 0, 8);

                        // Gửi dữ liệu
                        byte[] buffer = new byte[1024 * 1024]; // 1MB buffer
                        int bytesRead;
                        sentBytes = 0;
                        startTime = DateTime.Now;

                        while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            stream.Write(buffer, 0, bytesRead);
                            sentBytes += bytesRead;

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

                this.Invoke((Action)(() =>
                {
                    MessageBox.Show("Gửi thành công 100%!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    btnSend.Enabled = true;
                }));
            }
            catch (Exception ex)
            {
                this.Invoke((Action)(() =>
                {
                    MessageBox.Show("Lỗi: " + ex.Message, "Thất bại", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btnSend.Enabled = true;
                }));
            }
        }

        // Format số byte cho UI
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
