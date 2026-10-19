# Hướng dẫn thiết lập môi trường & chạy website (SETUP)

Tài liệu này giúp thành viên mới **chạy được website ngay sau khi clone** dự án `N3PTPMMNM`,
và tổng hợp **lỗi thường gặp** với cách giải quyết đã được kiểm chứng trên môi trường thật.

> Xem thêm: `README.md` (giới thiệu & build) — `CONTRIBUTING.md` (quy trình đóng góp).

---

## 1. Yêu cầu môi trường

| Thành phần | Ghi chú |
|---|---|
| Windows | IIS Express chỉ chạy trên Windows |
| Visual Studio | Có workload **ASP.NET and web development** (khuyên dùng bản ≥ 2019) |
| .NET Framework 4.5.2 Developer Pack | Dự án target `v4.5.2` |
| SQL Server | Bản bất kỳ: Developer / Express / MSSQLSERVER mặc định |
| SQL Server Management Studio (SSMS) | Để khôi phục database từ file backup (tùy chọn, có thể dùng `sqlcmd`) |

---

## 2. Các bước chạy lần đầu

### Bước 1 — Clone và mở solution

```bash
git clone https://github.com/Tuyen-CyberCode/N3PTPMMNM.git
cd N3PTPMMNM
```

Mở file `WebsiteNoiThat.sln` bằng Visual Studio. Chờ VS khôi phục NuGet packages
(thư mục `packages/` đã được track trong repo nên thường không cần restore thêm).

### Bước 2 — Khôi phục database từ file backup

Dự án dùng database `DBNoiThat` (SQL Server), file backup nằm ở gốc repo: `DBNoiThat.bak`.

Cách 1 — bằng SSMS:
1. Kết nối tới SQL Server local của bạn.
2. Chuột phải **Databases → Restore Database...**
3. **Device** → chọn file `DBNoiThat.bak` trong thư mục dự án.
4. Đặt **Destination database** = `DBNoiThat` → **OK**.

Cách 2 — bằng `sqlcmd` (đã có sẵn cùng SQL Server):

```bash
sqlcmd -S . -E -Q "RESTORE DATABASE DBNoiThat FROM DISK = N'D:\duong-dan\toi\N3PTPMMNM\DBNoiThat.bak' WITH REPLACE"
```

> `-S .` là instance mặc định trên máy bạn. Nếu dùng SQL Express, thay bằng `-S .\SQLEXPRESS`.

### Bước 3 — Sửa connection string cho đúng máy của bạn

Mở file `WebsiteNoiThat/Web.config`, tìm phần `<connectionStrings>`:

```xml
<add name="DBNoiThat"
     connectionString="data source=SAM;initial catalog=DBNoiThat;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework"
     providerName="System.Data.SqlClient" />
```

Đổi `data source=SAM` (máy của tác giả gốc) thành **SQL Server của máy bạn**:

| Máy của bạn có | `data source` viết là |
|---|---|
| SQL Server bản đầy đủ (instance mặc định) | `.` hoặc `localhost` hoặc tên máy |
| SQL Server Express | `.\SQLEXPRESS` hoặc `localhost\SQLEXPRESS` |
| Tên server khác | tên chính xác của instance |

> ⚠️ **Không commit thay đổi này lên Git.** Connection string là cấu hình riêng của từng máy.
> Hãy đưa thay đổi vào file `Web.Debug.config` (transform cục bộ) hoặc giữ ở máy mình —
> xem mục "Lỗi thường gặp" #3 dưới đây.

### Bước 4 — Build và chạy

1. **Build → Rebuild Solution** (`Ctrl+Shift+B`).
2. **F5** chạy project web (`WebsiteNoiThat`).
3. Trang chủ hiện danh sách sản phẩm = thành công.

---

## 3. Lỗi thường gặp & cách giải quyết

### Lỗi #1 — `HTTP Error 500.19` (mã `0x80070003`) khi mở website

**Triệu chứng:** trình duyệt hiện trang lỗi 500.19, phần "Config File" trong trang lỗi
chỉ về đường dẫn máy của người khác (vd `D:\CODE ...\NguyenThiNgoc_HTTT2\...`).

**Nguyên nhân:** thư mục `.vs/` (cache của Visual Studio) từng bị commit vào Git.
Bên trong có `applicationhost.config` ghi **cứng đường dẫn máy cũ**, nên khi bạn mở solution,
IIS Express cố chạy theo đường dẫn đó → không tồn tại trên máy bạn → lỗi.

**Giải pháp:**
1. Đóng Visual Studio.
2. Xóa thư mục `.vs` ở gốc repo.
3. Mở lại solution → VS tự sinh lại cấu hình đúng với máy của bạn.

> Nguyên nhân gốc đã được gỡ bỏ trong PR "chore: Thêm .gitignore và gỡ rác build (bin/obj/.vs)".
> Sau khi PR được merge, lỗi này không còn xảy ra.

### Lỗi #2 — `System.Data.DataException: An exception occurred while initializing the database`

**Triệu chứng:** exception khi mở trang chủ, call stack chỉ về các DAO (vd `ProductDao.NewProduct()`).

**Nguyên nhân:** connection string trong `Web.config` trỏ tới SQL Server không tồn tại trên máy bạn
(vd `data source=SAM`) — Entity Framework kết nối thất bại ở lần đầu khởi tạo.

**Giải pháp:** làm lại **Bước 3** ở trên — sửa `data source` về SQL Server local của bạn,
và xác nhận database `DBNoiThat` đã được khôi phục (**Bước 2**).

**Cách kiểm tra nhanh SQL Server của bạn đang có gì:**

```bash
sqlcmd -S . -E -Q "SELECT @@SERVERNAME; SELECT name FROM sys.databases"
```

### Lỗi #3 — Sau khi sửa `Web.config`, git liên tục báo file "modified"

**Nguyên nhân:** bạn vừa sửa `Web.config` (connection string máy mình) nhưng chưa được phép commit.

**Giải pháp tạm (giữ ở máy mình, không commit):**

```bash
git update-index --skip-worktree WebsiteNoiThat/Web.config
```

**Giải pháp dứt điểm (đề xuất cho cả nhóm):** thêm connection string riêng của từng người vào
`Web.Debug.config` (transform chỉ áp dụng khi Debug), còn `Web.config` giữ nguyên cấu hình chung.
Chờ nhóm thống nhất rồi mới đưa thành PR.

---

## 4. Checklist trước khi báo "chạy không được"

- [ ] Đã mở đúng `WebsiteNoiThat.sln` ở gốc repo.
- [ ] Build thành công, không còn lỗi đỏ trong Error List.
- [ ] SQL Server của máy đang chạy (kiểm tra trong SSMS hoặc `services.msc`).
- [ ] Database `DBNoiThat` đã tồn tại (mục Bước 2).
- [ ] `Web.config` trỏ `data source` đúng instance của máy (mục Bước 3).
- [ ] Nếu gặp 500.19 → đã xóa `.vs` và mở lại solution (Lỗi #1).

Gặp thêm lỗi mới? Hãy **tạo Issue** trên GitHub với đầy đủ:
tên lỗi (screenshot), config file / stack trace liên quan, và các bước đã thử —
nhóm sẽ cập nhật vào tài liệu này.
