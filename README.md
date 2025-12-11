# 📁 ĐỒ ÁN MÔN HỌC – LẬP TRÌNH MẠNG (LTM)

> **Đề tài:** Ứng dụng truyền file giữa hai máy tính sử dụng giao thức TCP (TCP File Transfer System).

## 📝 Giới thiệu

Hệ thống triển khai mô hình **Client - Server** cho phép truyền tải tập tin (mọi định dạng: `.mp4`, `.zip`, `.exe`,...) với dung lượng lớn qua mạng LAN. 

Hệ thống bao gồm 2 module chính:
* **Server:** Lắng nghe kết nối và nhận file.
* **Client:** Kết nối tới Server và gửi file.

💡 **Tính năng nổi bật:**
* Hỗ trợ truyền file Binary an toàn.
* Hiển thị **Real-time Progress**: % hoàn thành, dung lượng đã gửi.
* Tính toán **Tốc độ truyền tải (MB/s)** theo thời gian thực.
* Xử lý tên file tự động (Timestamp) để tránh ghi đè dữ liệu.

---

## 🛠 Công nghệ & Kỹ thuật sử dụng

* **Ngôn ngữ:** C# (.NET Framework 4.7.2 trở lên).
* **IDE:** Visual Studio 2019 / 2022.
* **Giao thức:** TCP/IP.
* **Core Technics:**
    * `System.Net.Sockets`: Sử dụng `TcpListener` (Server) và `TcpClient` (Client).
    * `System.IO`: Sử dụng `NetworkStream` và `FileStream` để xử lý luồng dữ liệu.
    * **Multithreading:** Xử lý đa luồng để Server có thể nhận nhiều kết nối hoặc không bị treo giao diện (UI) khi truyền tải.
    * **Buffer Management:** Tối ưu hóa buffer size (1MB) để tăng tốc độ đọc ghi.

---

## 🚀 Hướng dẫn cài đặt & Chạy

### 1️⃣ Khởi chạy Server
1.  Mở Solution `FileTransferSystem.sln` bằng Visual Studio.
2.  Chọn project **FileTransferServer** làm *Startup Project* (hoặc chuột phải -> Debug -> Start new instance).
3.  Nhấn **Start**.
4.  Server sẽ lắng nghe tại cổng mặc định **8080**.

### 2️⃣ Khởi chạy Client
1.  Chạy project **FileTransferClient**.
2.  Nhập thông tin kết nối:
    * **Server IP:** * `127.0.0.1` (nếu chạy cùng máy - Localhost).
        * `IP LAN` (ví dụ: `192.168.1.10`) nếu chạy khác máy.
    * **Port:** `8080`.
3.  Nhấn **Browse (...)** để chọn file cần gửi.
4.  Nhấn **Send / Upload** để bắt đầu.

---

## 🌐 Cấu hình mạng (LAN)

Để hai máy tính khác nhau trong cùng mạng LAN có thể truyền file, cần thực hiện:

1.  **Lấy IP của máy chạy Server:**
    Mở CMD, gõ lệnh:
    ```cmd
    ipconfig
    ```
    *Lấy địa chỉ IPv4 (ví dụ: 192.168.1.15).*

2.  **Mở port qua Firewall (trên máy Server):**
    Nếu Client không kết nối được, hãy chạy lệnh sau dưới quyền Administrator:
    ```cmd
    netsh advfirewall firewall add rule name="FileTransferServer" dir=in action=allow protocol=TCP localport=8080
    ```

---

## 📂 Lưu trữ dữ liệu

File sau khi Server nhận thành công sẽ được lưu tại:
`FileTransferServer/bin/Debug/Received/`

> **Lưu ý:** Tên file sẽ được tự động thêm **Timestamp** (HHmmss) vào phía trước để đảm bảo tính duy nhất (Ví dụ: `084450_hello.txt`).

---

## 👥 Thành viên thực hiện
1. Võ Phát Đạt
2. Vũ Mạnh Hùng
3. Dương Duy Quý
4. Trần Hoàng Phúc

---

