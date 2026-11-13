-- 1. Sửa bộ đếm UserID (lệnh RESEED lần trước đã đặt về 0 nên sẽ gây lỗi trùng ID)
DBCC CHECKIDENT ('tblUser', RESEED, 1012);

-- 2. Chuyển phiếu mượn (nếu có) của admin cũ sang mau123456 để xóa không bị lỗi khóa ngoại
UPDATE tblBorrowTicket
SET UserID = (SELECT UserID FROM tblUser WHERE UserName = N'mau123456')
WHERE UserID = (SELECT UserID FROM tblUser WHERE UserName = N'admin');

-- 3. Xóa admin cũ
DELETE FROM tblUser WHERE UserName = N'admin';

-- 4. Nâng mau123456 lên Admin
UPDATE tblUser SET Role = N'Admin', IsActive = 1 WHERE UserName = N'mau123456';

SELECT UserID, UserName, Role, IsActive FROM tblUser;