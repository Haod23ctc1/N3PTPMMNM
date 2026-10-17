# N3PTPMMNM - Website bán nội thất

## 1. Giới thiệu dự án

Website bán nội thất cho phép khách hàng xem, tìm kiếm và đặt mua sản phẩm trực tuyến. Dự án được xây dựng bằng ASP.NET MVC 5 trên nền .NET Framework.

## 2. Mục tiêu

- Trưng bày sản phẩm, danh mục và chương trình khuyến mãi.
- Hỗ trợ tìm kiếm sản phẩm, xem chi tiết và quản lý giỏ hàng.
- Cung cấp chức năng tài khoản khách hàng và quản lý đơn hàng/sản phẩm.

## 3. Chức năng

- Xem sản phẩm mới, sản phẩm khuyến mãi và sản phẩm theo danh mục.
- Tìm kiếm, lọc và xem chi tiết sản phẩm.
- Đăng ký, đăng nhập, quản lý tài khoản và lịch sử mua hàng.
- Thêm sản phẩm vào giỏ hàng, đặt hàng và theo dõi đơn hàng.
- Khu vực quản trị để quản lý sản phẩm, danh mục, người dùng và đơn hàng.

## 4. Công nghệ

- C#: ASP.NET MVC 5, .NET Framework 4.8.
- Entity Framework 6 và SQL Server.
- Razor, HTML, CSS, JavaScript, Bootstrap và jQuery.
- Visual Studio 2022 và IIS Express để phát triển/chạy ứng dụng.

## 5. Cài đặt dự án

### Yêu cầu

- Windows và Visual Studio, cài workload **ASP.NET and web development**.
- .NET Framework 4.8 Developer Pack.
- SQL Server và SQL Server Management Studio (SSMS).

### Cấu hình và chạy

Sao chép mã nguồn và mở solution:

```bash
git clone https://github.com/Tuyen-CyberCode/N3PTPMMNM.git
cd N3PTPMMNM
```

1. Mở `WebsiteNoiThat.sln` bằng Visual Studio.
2. Khôi phục database từ tệp `DBNoiThat.bak` trong SSMS, đặt tên database là `DBNoiThat`.
3. Kiểm tra chuỗi kết nối `DBNoiThat` trong `WebsiteNoiThat/Web.config`; sửa `data source` nếu SQL Server của bạn không dùng `localhost`.
4. Đặt **WebsiteNoiThat** làm Startup Project, chọn **IIS Express** rồi nhấn **F5**.

Để khôi phục NuGet packages và build trong **Developer PowerShell for Visual Studio**, tại thư mục gốc repository, chạy:

```powershell
nuget restore .\WebsiteNoiThat.sln
msbuild .\WebsiteNoiThat.sln /p:Configuration=Debug
```

Đây là ứng dụng ASP.NET MVC 5 chạy trên IIS Express, không phải ASP.NET Core; không chạy bằng `dotnet run`.

### Build và run bằng VS Code

1. Cài Visual Studio 2022 hoặc Build Tools với **MSBuild**, workload **ASP.NET and web development**, .NET Framework 4.8 Developer Pack và IIS Express.
2. Mở thư mục gốc chứa `WebsiteNoiThat.sln` trong VS Code. Nếu chưa có, cài extension **C# Dev Kit**.
3. Nếu NuGet packages chưa được khôi phục, chạy `nuget restore .\WebsiteNoiThat.sln` trong terminal (cần NuGet CLI).
4. Chọn **Terminal > Run Build Task** (hoặc `Ctrl+Shift+B`) và chọn **Build MVC 5 solution**.
5. Mở **Terminal > Run Task**, chọn **Run with IIS Express**, rồi truy cập localhost. Dừng máy chủ bằng `Ctrl+C` trong terminal của task.

Task sử dụng đường dẫn cài Visual Studio Community mặc định. Nếu dùng edition khác hoặc cài ở vị trí khác, sửa đường dẫn MSBuild trong `.vscode/tasks.json`. Đây là project ASP.NET MVC 5 kiểu cũ; `launchSettings.json` và `dotnet run` dành cho ASP.NET Core không khởi chạy project này.

VS Code có thể hiển thị chẩn đoán Razor sai như `ViewBag`, `Write` hoặc `WriteLiteral` không tồn tại trong `.cshtml` vì dự án MVC 5 trên .NET Framework không dùng Razor tooling của ASP.NET Core. Không sửa các view để xử lý những chẩn đoán giả này: xác nhận bằng build task; nếu cần IntelliSense/kiểm tra Razor đầy đủ, mở solution bằng Visual Studio 2022.

## 6. Quy trình đóng góp

1. Sao chép kho mã nguồn về máy.
2. Tạo nhánh riêng cho công việc.
3. Thực hiện thay đổi mã nguồn.
4. Kiểm tra các thay đổi.
5. Commit những tệp cần thiết.
6. Push nhánh lên GitHub.
7. Tạo Pull Request để đề xuất hợp nhất vào nhánh `main`.
8. Chờ người quản lý dự án xem xét và phản hồi.

Xem hướng dẫn chi tiết tại [CONTRIBUTING.md](CONTRIBUTING.md).

## 7. Thành viên

Danh sách thành viên và vai trò sẽ được cập nhật theo thông tin chính thức của nhóm.

## 8. Giấy phép

Giấy phép của dự án sẽ được bổ sung theo quy định của kho mã nguồn.

## 9. Build và chạy ứng dụng

Xem các yêu cầu, hướng dẫn khôi phục database và lệnh build tại mục **5. Cài đặt dự án**.
