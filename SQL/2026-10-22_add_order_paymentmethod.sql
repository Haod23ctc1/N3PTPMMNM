-- 2026-10-22: Bổ sung cột lưu phương thức thanh toán cho bảng Order (issues #67)
-- Chạy 1 lần trên database DBNoiThat trước khi deploy phiên bản có mã này.
USE DBNoiThat
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns c WHERE c.object_id = OBJECT_ID(N'[Order]') AND c.name = 'PaymentMethod')
BEGIN
    ALTER TABLE [Order] ADD PaymentMethod nvarchar(50) NULL;
END
GO
