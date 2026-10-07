using BCrypt.Net;
using DigitalLearningQA.Domain.Entities;

namespace DigitalLearningQA.Infrastructure.Persistence.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (!context.Roles.Any())
        {
            context.Roles.AddRange(
                new Role
                {
                    Name = "Admin",
                    Description = "Quản trị hệ thống"
                },
                new Role
                {
                    Name = "CCD",
                    Description = "Trung tâm học liệu số"
                },
                new Role
                {
                    Name = "Lecturer",
                    Description = "Giảng viên"
                });

            await context.SaveChangesAsync();
        }

        var departments = new[]
        {
            ("CNTT", "Khoa Công nghệ thông tin"), ("KTDN", "Khoa Kinh tế - Quản trị kinh doanh"),
            ("NN", "Khoa Ngoại ngữ"), ("DL", "Khoa Du lịch"), ("SP", "Khoa Sư phạm"),
            ("CN", "Khoa Công nghệ sinh học"), ("XD", "Khoa Kỹ thuật công trình"),
            ("LLCT", "Khoa Lý luận chính trị"), ("Y", "Khoa Y - Dược"),
            ("KTCN", "Khối Kỹ thuật - Công nghệ"), ("XHNV", "Khối Khoa học Xã hội và Nhân văn"),
            ("TT", "Khoa Truyền thông"), ("TK", "Khoa Mỹ thuật và Thiết kế")
        };
        foreach (var (code, name) in departments)
            if (!context.Departments.Any(x => x.Code == code))
                context.Departments.Add(new Department { Code = code, Name = name, Description = name, IsActive = true });
        await context.SaveChangesAsync();

        var majors = new[]
        {
            // Danh mục chương trình đại học chính quy Đại học Đại Nam, năm 2026.
            ("Y", "7720101", "Y khoa"), ("Y", "7720101-QT", "Y khoa (đào tạo bằng tiếng Anh)"),
            ("Y", "7720201", "Dược học"), ("Y", "7720301", "Điều dưỡng"),
            ("CNTT", "7480201", "Công nghệ thông tin"), ("CNTT", "7480101", "Khoa học máy tính"),
            ("CNTT", "7480104", "Hệ thống thông tin"),
            ("KTCN", "7510205", "Công nghệ kỹ thuật ô tô"), ("KTCN", "7510301", "Công nghệ kỹ thuật điện - điện tử"),
            ("KTCN", "7510203", "Công nghệ kỹ thuật cơ điện tử"), ("KTCN", "7510303", "Công nghệ kỹ thuật và điều khiển tự động hoá"),
            ("KTCN", "7510303-BD", "Công nghệ bán dẫn"), ("XD", "7580101", "Kiến trúc"),
            ("XD", "7580201", "Kỹ thuật xây dựng"), ("XD", "7580302", "Kinh tế xây dựng"),
            ("KTDN", "7340101", "Quản trị kinh doanh"), ("KTDN", "7340404", "Quản trị nhân lực"),
            ("KTDN", "7340115", "Marketing"), ("KTDN", "7340120", "Kinh doanh quốc tế"),
            ("KTDN", "7340122", "Thương mại điện tử"), ("KTDN", "7310101", "Kinh tế"),
            ("KTDN", "7310109", "Kinh tế số"), ("KTDN", "7340201", "Tài chính - Ngân hàng"),
            ("KTDN", "7340205", "Công nghệ tài chính (Fintech)"), ("KTDN", "7510605", "Logistics và quản lý chuỗi cung ứng"),
            ("KTDN", "7340301", "Kế toán"), ("XHNV", "7310401", "Tâm lý học"),
            ("XHNV", "7380101", "Luật"), ("XHNV", "7380107", "Luật kinh tế"),
            ("DL", "7810103", "Quản trị dịch vụ du lịch và lữ hành"), ("DL", "7810201", "Quản trị khách sạn"),
            ("TT", "7320104", "Truyền thông đa phương tiện"), ("TT", "7320108", "Quan hệ công chúng"),
            ("NN", "7220201", "Ngôn ngữ Anh"), ("NN", "7220204", "Ngôn ngữ Trung Quốc"),
            ("NN", "7220210", "Ngôn ngữ Hàn Quốc"), ("NN", "7220209", "Ngôn ngữ Nhật"),
            ("TK", "7210403", "Thiết kế đồ hoạ")
        };
        foreach (var (departmentCode, code, name) in majors)
        {
            var departmentForMajor = context.Departments.First(x => x.Code == departmentCode);
            if (!context.Majors.Any(x => x.DepartmentId == departmentForMajor.Id && x.Code == code))
                context.Majors.Add(new Major { DepartmentId = departmentForMajor.Id, Code = code, Name = name, IsActive = true });
        }
        await context.SaveChangesAsync();

        var criteria = new[]
        {
            ("KT-01", "Tệp học liệu mở được và không bị lỗi", "Kỹ thuật", "Kiểm tra khả năng truy cập tệp hoặc liên kết học liệu.", (int?)null, 2, true),
            ("KT-02", "Định dạng tệp phù hợp với LMS", "Kỹ thuật", "Định dạng tương thích và dung lượng phù hợp để tải lên LMS.", (int?)null, 1, true),
            ("ND-01", "Tên học liệu và nội dung đúng học phần", "Nội dung", "Tên, chủ đề và nội dung cốt lõi khớp với học phần.", (int?)null, 2, true),
            ("SP-01", "Có hướng dẫn hoặc hoạt động học tập", "Sư phạm", "Học liệu có hướng dẫn sử dụng, hoạt động hoặc câu hỏi hỗ trợ người học.", (int?)null, 2, true),
            ("OBE-01", "Có liên kết chuẩn đầu ra CLO", "OBE", "Nêu rõ CLO hoặc minh chứng liên kết chuẩn đầu ra của học phần.", (int?)null, 3, true),
            ("OBE-02", "Có công cụ đánh giá hoặc minh chứng CLO", "OBE", "Có câu hỏi, bài tập, rubric hoặc minh chứng đánh giá CLO.", (int?)null, 3, true),
            ("VD-01", "Âm thanh và hình ảnh rõ ràng", "Kỹ thuật", "Áp dụng với video bài giảng.", 3, 2, true),
            ("SL-01", "Bố cục slide rõ ràng, dễ đọc", "Sư phạm", "Áp dụng với tài liệu trình chiếu.", 2, 2, true)
        };
        foreach (var (code, name, category, description, materialType, weight, required) in criteria)
            if (!context.QualityCriteria.Any(x => x.Code == code))
                context.QualityCriteria.Add(new QualityCriterion { Code = code, Name = name, Category = category, Description = description, MaterialType = materialType is null ? null : (Domain.Enums.MaterialType)materialType, Weight = weight, IsRequired = required, IsActive = true });
        await context.SaveChangesAsync();

        var department = context.Departments.First();
        // Chỉ tạo Admin khởi tạo. Tài khoản CCD/Giảng viên phải được Admin cấp
        // qua API /api/users để phản ánh đúng nghiệp vụ, không sinh dữ liệu giả.
        var demoUsers = new[]
        {
            ("admin", "System Administrator", "admin@dlu.edu.vn", "Admin", "123456", false)
        };

        foreach (var (username, fullName, email, roleName, password, isFirstLogin) in demoUsers)
        {
            if (context.Users.Any(x => x.Username == username))
                continue;

            var role = context.Roles.First(r => r.Name == roleName);
            context.Users.Add(new User
            {
                RoleId = role.Id,
                DepartmentId = department.Id,
                Username = username,
                FullName = fullName,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Status = Domain.Enums.UserStatus.Active,
                IsFirstLogin = isFirstLogin
            });
        }

        await context.SaveChangesAsync();
    }
}
