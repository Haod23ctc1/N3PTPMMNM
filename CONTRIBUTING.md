# CONTRIBUTING.md

# Hướng dẫn đóng góp cho dự án website nội thất N3PTPMMNM

Cảm ơn bạn đã tham gia đóng góp cho dự án website bán nội thất của nhóm. Tài liệu này hướng dẫn các thành viên cách làm việc với mã nguồn, kiểm tra thay đổi và gửi Pull Request.

## 1. Công nghệ dự án

Dự án website nội thất tham khảo mô hình ASP.NET MVC, C# và SQL Server.

Các phần chức năng có thể bao gồm:
- Quản lý tài khoản và đăng nhập.
- Hiển thị và quản lý sản phẩm nội thất.
- Giỏ hàng và đơn hàng.
- Quản lý nhân viên và chức năng quản trị.
- Lưu trữ và truy xuất dữ liệu bằng SQL Server.

Thành viên cần kiểm tra cấu trúc và công nghệ thực tế trong repository của nhóm trước khi sửa code.

## 2. Chuẩn bị môi trường

- Cài đặt Git và Visual Studio Code.
- Cài đặt các công cụ .NET, SQL Server hoặc phần mềm cần thiết theo yêu cầu thực tế của dự án.
- Clone repository:

  ```bash
  git clone https://github.com/Tuyen-CyberCode/N3PTPMMNM.git
  ```

- Mở thư mục dự án trong VS Code.
- Kiểm tra trạng thái trước khi làm việc:

  ```bash
  git status
  ```

## 3. Quy trình làm việc với branch

Không tự ý sửa trực tiếp branch main. Mỗi thành viên nên làm việc trên branch riêng.

Cập nhật main trước khi tạo branch mới:

```bash
git switch main
git pull origin main
git switch -c feature/ten-chuc-nang
```

Quy ước tên branch:

- `feature/ten-chuc-nang`: phát triển chức năng mới.
- `fix/ten-loi`: sửa lỗi.
- `docs/noi-dung`: cập nhật tài liệu.

Ví dụ:

- `feature/product-management`
- `feature/shopping-cart`
- `fix/login-validation`
- `docs/contributing-guide`

Nếu nhóm có quy ước đặt tên branch riêng, hãy tuân thủ quy ước của nhóm.

## 4. Quy tắc viết code

- Viết code dễ đọc, dễ hiểu và dễ bảo trì.
- Đặt tên biến, phương thức, lớp và file phù hợp với chức năng.
- Giữ cấu trúc và phong cách code nhất quán với dự án hiện có.
- Chỉ sửa các file liên quan đến nhiệm vụ được giao.
- Kiểm tra ảnh hưởng của thay đổi đến giao diện, xử lý nghiệp vụ và cơ sở dữ liệu.
- Không đưa mật khẩu, thông tin kết nối nhạy cảm hoặc khóa bí mật vào repository.
- Không commit file cấu hình cá nhân hoặc file build tự sinh nếu chúng không cần thiết cho dự án.

## 5. Kiểm tra trước khi commit

Kiểm tra các file đã thay đổi:

```bash
git status
git diff
```

Chỉ đưa các file cần thiết vào staging:

```bash
git add duong-dan-file
```

Kiểm tra nội dung chuẩn bị commit:

```bash
git diff --cached
```

Đối với thay đổi liên quan đến C# hoặc SQL Server, cần kiểm tra khả năng build, chạy thử chức năng và xác nhận dữ liệu hoạt động đúng khi môi trường cho phép.

## 6. Quy tắc commit

Thông điệp commit cần ngắn gọn, mô tả đúng nội dung thay đổi.

Ví dụ:

```bash
git commit -m "Add product management"
git commit -m "Fix login validation"
git commit -m "Update contributing guide"
```

Không đưa các thay đổi không liên quan vào cùng một commit.

## 7. Push code lên GitHub

Sau khi commit thành công, push branch của mình:

```bash
git push -u origin ten-branch
```

Ví dụ:

```bash
git push -u origin feature/product-management
```

Thay tên branch bằng branch thực tế đang sử dụng.

## 8. Tạo Pull Request

Sau khi push code:

1. Mở repository trên GitHub.
2. Tạo Pull Request từ branch cá nhân vào branch main.
3. Đặt tiêu đề ngắn gọn và mô tả chức năng đã thực hiện.
4. Nêu các thay đổi chính và kết quả kiểm thử.
5. Chờ Maintainer hoặc người có quyền review.
6. Chỉnh sửa theo góp ý nếu cần.
7. Chỉ merge theo quy trình và quyền hạn mà nhóm quy định.

Không tự ý merge Pull Request khi chưa được nhóm cho phép.

## 9. Checklist trước khi gửi Pull Request

- [ ] Đã hoàn thành chức năng được giao.
- [ ] Đã kiểm tra các thay đổi bằng git diff.
- [ ] Đã chạy thử hoặc kiểm thử phù hợp.
- [ ] Không đưa thông tin bí mật vào repository.
- [ ] Không đưa file build hoặc file cá nhân không cần thiết lên GitHub.
- [ ] Commit có thông điệp rõ ràng.
- [ ] Pull Request có mô tả đầy đủ.
- [ ] Đã thông báo cho các thành viên liên quan nếu thay đổi ảnh hưởng đến phần code chung.

## 10. Phối hợp trong nhóm

- Trao đổi với nhóm khi yêu cầu chức năng chưa rõ.
- Không tự ý ghi đè hoặc xóa code của thành viên khác.
- Thống nhất thay đổi liên quan đến cơ sở dữ liệu trước khi thực hiện.
- Tuân thủ hướng dẫn của Maintainer và quy định của nhóm.

Mục tiêu của tài liệu là giúp các thành viên phối hợp hiệu quả, quản lý thay đổi rõ ràng và duy trì chất lượng mã nguồn của dự án.
