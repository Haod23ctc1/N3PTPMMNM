-- 2026-11-01: Tao bang ChatMessage cho tinh nang chat noi bo + OTP qua kenh Chat (issue #114)
-- Lenh chay: sqlcmd -S . -E -d DBNoiThat -f 65001 -i SQL\2026-11-01_tao_bang_chatmessage.sql
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'ChatMessage')
BEGIN
    CREATE TABLE [dbo].[ChatMessage](
        [ChatMessageId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [UserId]        INT            NULL,
        [Email]         NVARCHAR(100)  NULL,
        [Phone]         NVARCHAR(15)   NULL,
        [Sender]        NVARCHAR(50)   NOT NULL,
        [Content]       NVARCHAR(4000) NOT NULL,
        [IsSystem]      BIT            NOT NULL DEFAULT 0,
        [Status]        BIT            NULL,
        [CreatedDate]   DATETIME       NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [FK_ChatMessage_User] FOREIGN KEY ([UserId]) REFERENCES [dbo].[User] ([UserId])
    );
END
GO

-- Index de tra cuu thread theo UserId / Email
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ChatMessage_UserId')
    CREATE INDEX [IX_ChatMessage_UserId] ON [dbo].[ChatMessage] ([UserId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ChatMessage_Email')
    CREATE INDEX [IX_ChatMessage_Email] ON [dbo].[ChatMessage] ([Email]);
GO
