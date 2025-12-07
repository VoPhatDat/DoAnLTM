📁 ĐỒ ÁN MÔN HỌC – LẬP TRÌNH MẠNG (LTM)
⚡ Ứng dụng truyền file giữa hai máy tính sử dụng TCP
📝 Giới thiệu

Đồ án triển khai một hệ thống truyền tập tin dung lượng lớn giữa hai máy tính thông qua giao thức TCP.
Hệ thống gồm 2 chương trình:

Client → Gửi file

Server → Nhận file

Ứng dụng hỗ trợ truyền file mọi định dạng, hiển thị tiến trình % và tốc độ truyền theo thời gian thực.

🚀 Cách chạy chương trình
1️⃣ Chạy Server trước

Mở solution:

FileTransferSystem.sln


Chọn project:

FileTransferServer


Nhấn Start để bật server.

Server sẽ chạy trên port 8080 và hiển thị log khi có client kết nối.

2️⃣ Chạy Client để gửi file

Chạy project:

FileTransferClient


Nhập IP Server:

Nếu chạy cùng máy → 127.0.0.1

Nếu chạy máy khác trong LAN → nhập IPv4 thật (vd: 192.168.1.10)

Nhấn Browse để chọn file.

Nhấn Send để bắt đầu truyền file.

Xem tiến trình:

% hoàn thành

Số byte đã gửi

Tốc độ gửi (MB/s)

🌐 Kết nối qua LAN

Để máy khác gửi file tới Server, cần:

✔ Xác định IP Server

Mở CMD và chạy:

ipconfig


Lấy IPv4 (ví dụ: 192.168.1.15)

✔ Mở cổng Firewall 8080 (nếu cần)
netsh advfirewall firewall add rule name="FileTransferServer" dir=in action=allow protocol=TCP localport=8080

✔ Client nhập đúng IP và nhấn Send
📂 Thư mục lưu file nhận được

Server sẽ lưu file vào thư mục:

FileTransferServer/bin/Debug/Received/


Tên file được tự động thêm timestamp để tránh trùng lặp.

🧠 Các kỹ thuật đã sử dụng

TCP Socket: TcpClient, TcpListener

Luồng dữ liệu: NetworkStream, FileStream

Truyền dữ liệu dạng nhị phân (binary stream)

Đa luồng (Thread) để xử lý nhiều client đồng thời

Giao diện WinForms (progress bar, log real-time)

Xử lý buffer lớn (1MB) để tối ưu tốc độ

🛠 Yêu cầu môi trường

Windows

.NET Framework 4.7.2 trở lên

Visual Studio 2022 / 2019

👥 Thành viên nhóm

Võ Phát Đạt

(Thêm tên bạn nếu cần)

✅ Trạng thái dự án

Đã hoàn thiện và kiểm thử với file nhỏ đến file lớn (mp4, zip, exe…).
