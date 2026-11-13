-- ========================================================
-- SCRIPT TẠO / NÂNG CẤP TÀI KHOẢN ADMIN ĐỂ PHÂN QUYỀN
-- Database: LibraryDB
-- ========================================================

USE [LibraryDB]
GO

-- 1. Kiểm tra tài khoản 'admin' đã tồn tại chưa
IF EXISTS (SELECT 1 FROM [tblUser] WHERE [UserName] = N'admin')
BEGIN
    -- Cập nhật quyền Admin và kích hoạt tài khoản
    UPDATE [tblUser]
    SET [Role] = N'Admin',
        [IsActive] = 1,
        [FullName] = N'Nguyễn Quản Trị (Admin)',
        [Email] = N'admin@thuvien.edu.vn',
        -- Mật khẩu MD5 của '123456'
        [Password] = N'2a417fee7c18e2f58b645e032084e11a'
    WHERE [UserName] = N'admin';

    PRINT N'Đã cập nhật tài khoản [admin] lên quyền Admin thành công!';
END
ELSE
BEGIN
    -- Thêm mới tài khoản Admin nếu chưa có
    INSERT INTO [tblUser] ([UserName], [FullName], [Email], [Password], [Role], [IsActive])
    VALUES (N'admin', N'Nguyễn Quản Trị (Admin)', N'admin@thuvien.edu.vn', N'2a417fee7c18e2f58b645e032084e11a', N'Admin', 1);

    PRINT N'Đã tạo mới tài khoản [admin] với quyền Admin thành công!';
END
GO

-- 2. Thêm tài khoản Thủ thư mẫu để thử nghiệm phân quyền
IF NOT EXISTS (SELECT 1 FROM [tblUser] WHERE [UserName] = N'thuthu01')
BEGIN
    INSERT INTO [tblUser] ([UserName], [FullName], [Email], [Password], [Role], [IsActive])
    VALUES (N'thuthu01', N'Trần Thị Thủ Thư', N'thuthu@thuvien.edu.vn', N'2a417fee7c18e2f58b645e032084e11a', N'Librarian', 1);

    PRINT N'Đã tạo thêm tài khoản Thủ thư [thuthu01]!';
END
GO

-- 3. Kiểm tra danh sách người dùng và phân quyền
SELECT [UserID], [UserName], [FullName], [Email], [Role], [IsActive]
FROM [tblUser]
ORDER BY [UserID] DESC;
GO
