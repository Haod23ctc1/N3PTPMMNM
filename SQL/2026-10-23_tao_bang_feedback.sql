-- 2026-10-23: Tao bang Feedbacks cho tinh nang danh gia san pham (issue #76)
-- Lenh chay: sqlcmd -S . -E -d DBNoiThat -f 65001 -i SQL\2026-10-23_tao_bang_feedback.sql
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'Feedbacks')
BEGIN
    CREATE TABLE [dbo].[Feedbacks](
        [FeedbackId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [ProductId]  INT            NOT NULL,
        [UserId]     INT            NOT NULL,
        [Rate]       INT            NOT NULL,
        [Content]    NVARCHAR(MAX)  NOT NULL,
        [Time]       DATETIME       NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [FK_Feedback_Product] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Product] ([ProductId]),
        CONSTRAINT [FK_Feedback_User]    FOREIGN KEY ([UserId])    REFERENCES [dbo].[User] ([UserId])
    );
END
GO

-- Du lieu mau de demo tinh nang (chi tao khi trong DB da co toi thieu 2 tai khoan)
IF NOT EXISTS (SELECT TOP 1 1 FROM Feedbacks)
BEGIN
    ;WITH u AS (
        SELECT TOP (2) ROW_NUMBER() OVER (ORDER BY UserId) AS rn, UserId FROM [User]
    )
    INSERT INTO Feedbacks (ProductId, UserId, Rate, Content, Time)
    SELECT 20, u.UserId, x.Rate, x.Content, DATEADD(DAY, -x.DayBack, GETDATE())
    FROM u
    JOIN (VALUES
            (1, 5, N'Trên hình đã thấy đẹp mà nhận hàng còn đẹp hơn nữa. Mặt gỗ mịn, tay nghề chạm khắc cực kỳ tỉ mỉ. Cả nhà đều mê, đáng giá từng đồng.', 3),
            (2, 4, N'Hàng đóng gói kỹ, vận chuyển an toàn. Màu gỗ lên thật, chi tiết chạm khắc rõ nét. Sẽ giới thiệu cho bạn bè cùng mua.', 1)
         ) AS x(rn, Rate, Content, DayBack) ON u.rn = x.rn;
END
GO
