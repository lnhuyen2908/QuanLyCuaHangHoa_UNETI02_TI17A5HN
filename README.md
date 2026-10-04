# 🌸 DỰ ÁN QUẢN LÝ CỬA HÀNG HOA FLORISSA (UNETI)

---

## 1. 🚀 Tải Dự Án & Cấu Hình Ban Đầu

```bash
# 1. Clone dự án về máy
git clone https://github.com/<tai-khoan-nhom>/QuanLyCuaHangHoa_UNETI02_TI17A5HN.git
cd QuanLyCuaHangHoa_UNETI02_TI17A5HN

# 2. Phục hồi thư viện & Cập nhật Database SQL Server
dotnet restore
dotnet ef database update

# 3. Khởi chạy dự án
dotnet run
```

> **Lưu ý:** Sửa lại `ConnectionStrings` trong file `appsettings.json` cho đúng với tên SQL Server trên máy bạn trước khi chạy `dotnet ef database update`.

---

## 2. 📌 Cú Pháp Tên Nhánh & Commit (Bắt Buộc Chấm Điểm)

### 🔹 Đặt tên nhánh:

`git checkout -b <tên-bạn>/<module>-<tính-năng>`  
_Ví dụ:_ `huyen/module1-dang-nhap`

### 🔹 Đặt câu lệnh Commit:

`git commit -m "[<MãSV>-<HọTên>] [<Module>] <Nội dung công việc>"`  
_Ví dụ:_ `git commit -m "[21103100123-Lã Ngọc Huyền] [Module 1] Tạo ViewModel và View loại hoa"`

---

## 3. 💻 Quy Trình Làm Việc Với Git (Từ A -> Z)

### Bước 1: Kéo code mới nhất từ main về

```bash
git checkout main
git pull origin main
```

### Bước 2: Tạo nhánh tính năng mới

```bash
git checkout -b huyen/module1-loai-hoa
```

### Bước 3: Lưu và Commit công việc

```bash
# Kiểm tra file đã sửa
git status

# Đưa tất cả file vào Staging
git add .

# Commit đúng chuẩn chấm điểm
git commit -m "[21103100123-Lã Ngọc Huyền] [Module 1] Hoàn thiện giao diện loại hoa"
```

### Bước 4: Cập nhật code mới nhất từ main về nhánh mình (Tránh xung đột)

```bash
git fetch origin
git merge origin/main
```

_(Nếu bị Conflict: Sửa file trên VS/VS Code -> `git add .` -> `git commit -m "Fix conflict"`)_

### Bước 5: Push lên GitHub & Tạo Pull Request (Nhóm trưởng thực hiện)

```bash
git push origin huyen/module1-loai-hoa
```

-> Truy cập trang GitHub của dự án -> Bấm **Compare & pull request** -> Merge vào nhánh `main`.

### Bước 6: Dọn dẹp nhánh cũ sau khi đã Merge

```bash
git checkout main
git pull origin main
git branch -d huyen/module1-loai-hoa
```

---

## 4. ⚡ Câu Lệnh Phụ Thường Dùng

```bash
# Xem tất cả các nhánh
git branch -a

# Hủy bỏ các thay đổi chưa commit
git checkout -- .

# Đồng bộ lại Database khi kéo code mới từ nhóm về
dotnet ef database update
```
