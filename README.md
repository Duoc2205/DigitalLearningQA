# Hệ thống kiểm thử và nghiệm thu học liệu số

Hệ thống quản lý học liệu theo OBE với ba vai trò: Quản trị viên, CCD và Giảng viên.

## Chức năng chính

- Quản trị viên cấp, khóa, xóa và đặt lại mật khẩu tài khoản.
- Giảng viên và CCD nộp học liệu, sửa và nộp phiên bản mới.
- Hệ thống đọc nội dung thực từ PDF, DOCX, PPTX, HTML và gói SCORM.
- Đánh giá theo bốn nhóm: Kỹ thuật, Nội dung, Sư phạm và OBE; tổng điểm 100.
- AI nêu lỗi, kiến nghị, kết luận đạt/chưa đạt; người nộp có thể xuất CSV hoặc in/lưu báo cáo PDF.
- Lưu lịch sử phiên bản và chênh lệch điểm sau mỗi lần nộp lại.
- Khi đạt từ 80 điểm và không còn lỗi mức cao, Giảng viên hoặc CCD xác nhận nghiệm thu theo báo cáo AI.

## Chạy backend

Thiết lập chuỗi kết nối SQL Server trong `backend/DigitalLearningQA.API/appsettings.json` và đặt khóa JWT qua biến môi trường `Jwt__Key` (không đưa khóa thật vào Git), sau đó:

```powershell
$env:Jwt__Key = "your-long-random-secret"
cd backend
dotnet run --project .\DigitalLearningQA.API\DigitalLearningQA.API.csproj
```

API mặc định chạy tại `http://localhost:5293`; Swagger có tại `/swagger` khi chạy Development.

## Chạy frontend

```powershell
cd frontend
npm.cmd install
npm.cmd run dev
```

Mở `http://localhost:5173`.

## AI ngữ nghĩa tùy chọn

Không lưu khóa vào mã nguồn. Đặt khóa trong biến môi trường Windows, rồi mở terminal mới trước khi chạy backend:

```powershell
setx OPENAI_API_KEY "sk-..."
```

Khi có khóa hợp lệ, backend gọi OpenAI để đánh giá ngữ nghĩa của nội dung đã trích xuất. Nếu chưa cấu hình khóa hoặc dịch vụ không phản hồi, hệ thống vẫn chấm theo bằng chứng rõ ràng trong tệp và báo đúng phương pháp đã dùng.

## PDF quét ảnh (OCR)

PDF quét ảnh không có lớp chữ cần OCR trước khi đánh giá nội dung. Hệ thống sẽ ghi rõ trường hợp này trong báo cáo, không tự nhận là đã đọc được tệp. Để bật OCR tại máy chủ, cài Tesseract OCR và gói ngôn ngữ Việt, bảo đảm lệnh `tesseract` có trong `PATH`, sau đó khởi động lại backend.

## Xuất PDF

Trong màn hình kết quả AI, bấm **Xuất báo cáo AI**. Ở hộp thoại in của trình duyệt, chọn **Save as PDF / Lưu dưới dạng PDF**.

## Kiểm thử tự động API

Script không cần cài thêm thư viện nằm tại `tests/api-smoke.ps1`. Chạy các kiểm tra an toàn về API ẩn danh trước:

```powershell
.\tests\api-smoke.ps1
```

Để kiểm tra đăng nhập, JWT, thông báo và phân quyền, đặt thông tin tài khoản kiểm thử trong biến môi trường trước khi chạy. Luồng upload và chấm chỉ chạy khi truyền `-RunWriteFlow`, vì nó tạo học liệu thật trong cơ sở dữ liệu.
