-- =====================================================================
-- SCRIPT TẠO & CẬP NHẬT CÁC MÃ ĐỊNH DANH HỆ THỐNG THƯ VIỆN:
-- 1. Mã người dùng / bạn đọc: UserCode (VD: DG0001, TT0001, AD0001)
-- 2. Mã sách: BookCode (VD: MS0001, MS0002)
-- 3. Mã phiếu mượn: TicketCode (VD: PM0001, PM-202611-001)
-- 4. Mã trả sách / phiếu trả: ReturnCode (VD: PT0001, PT-202611-001)
-- Database: LibraryDB
-- =====================================================================

USE [LibraryDB]
GO

-- 1. THÊM CỘT [UserCode] VÀO BẢNG [tblUser] NẾU CHƯA CÓ
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('tblUser') AND name = 'UserCode')
BEGIN
    ALTER TABLE [dbo].[tblUser] ADD [UserCode] NVARCHAR(30) NULL;
    PRINT N'Đã thêm cột [UserCode] vào bảng [tblUser] thành công!';
END
GO

-- 2. THÊM CỘT [BookCode] VÀO BẢNG [tblBook] NẾU CHƯA CÓ
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('tblBook') AND name = 'BookCode')
BEGIN
    ALTER TABLE [dbo].[tblBook] ADD [BookCode] NVARCHAR(30) NULL;
    PRINT N'Đã thêm cột [BookCode] vào bảng [tblBook] thành công!';
END
GO

-- 3. THÊM CỘT [TicketCode] VÀ [ReturnCode] VÀO BẢNG [tblBorrowTicket] NẾU CHƯA CÓ
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('tblBorrowTicket') AND name = 'TicketCode')
BEGIN
    ALTER TABLE [dbo].[tblBorrowTicket] ADD [TicketCode] NVARCHAR(30) NULL;
    PRINT N'Đã thêm cột [TicketCode] vào bảng [tblBorrowTicket] thành công!';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('tblBorrowTicket') AND name = 'ReturnCode')
BEGIN
    ALTER TABLE [dbo].[tblBorrowTicket] ADD [ReturnCode] NVARCHAR(30) NULL;
    PRINT N'Đã thêm cột [ReturnCode] vào bảng [tblBorrowTicket] thành công!';
END
GO

-- =====================================================================
-- CẬP NHẬT DỮ LIỆU MẪU CHO CÁC BẢNG ĐÃ CÓ DỮ LIỆU
-- =====================================================================

-- Cập nhật UserCode cho người dùng hiện tại
UPDATE [dbo].[tblUser]
SET [UserCode] = 'AD' + RIGHT('0000' + CAST([UserID] AS VARCHAR(10)), 4)
WHERE [Role] = 'Admin' AND ([UserCode] IS NULL OR [UserCode] = '');

UPDATE [dbo].[tblUser]
SET [UserCode] = 'TT' + RIGHT('0000' + CAST([UserID] AS VARCHAR(10)), 4)
WHERE [Role] = 'Librarian' AND ([UserCode] IS NULL OR [UserCode] = '');

UPDATE [dbo].[tblUser]
SET [UserCode] = 'DG' + RIGHT('0000' + CAST([UserID] AS VARCHAR(10)), 4)
WHERE ([Role] IS NULL OR [Role] = 'Reader') AND ([UserCode] IS NULL OR [UserCode] = '');

PRINT N'Đã cập nhật Mã người dùng (UserCode) cho toàn bộ người dùng!';

-- Cập nhật BookCode cho kho sách
UPDATE [dbo].[tblBook]
SET [BookCode] = 'MS' + RIGHT('0000' + CAST([BookID] AS VARCHAR(10)), 4)
WHERE [BookCode] IS NULL OR [BookCode] = '';

PRINT N'Đã cập nhật Mã sách (BookCode) cho toàn bộ sách trong kho!';

-- Cập nhật TicketCode cho các phiếu mượn
UPDATE [dbo].[tblBorrowTicket]
SET [TicketCode] = 'PM' + RIGHT('0000' + CAST([TicketID] AS VARCHAR(10)), 4)
WHERE [TicketCode] IS NULL OR [TicketCode] = '';

-- Cập nhật ReturnCode cho các phiếu đã trả sách
UPDATE [dbo].[tblBorrowTicket]
SET [ReturnCode] = 'PT' + RIGHT('0000' + CAST([TicketID] AS VARCHAR(10)), 4)
WHERE [Status] = N'Đã trả' AND ([ReturnCode] IS NULL OR [ReturnCode] = '');

PRINT N'Đã cập nhật Mã phiếu mượn (TicketCode) và Mã trả sách (ReturnCode)!';
GO

-- KIỂM TRA KẾT QUẢ
SELECT TOP 5 [UserID], [UserCode], [UserName], [FullName], [Role] FROM [dbo].[tblUser] ORDER BY [UserID];
SELECT TOP 5 [BookID], [BookCode], [Title], [AvailableQuantity] FROM [dbo].[tblBook] ORDER BY [BookID];
SELECT TOP 5 [TicketID], [TicketCode], [ReturnCode], [Status], [BorrowDate], [ReturnDate] FROM [dbo].[tblBorrowTicket] ORDER BY [TicketID] DESC;
GO
