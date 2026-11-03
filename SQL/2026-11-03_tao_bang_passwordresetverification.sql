-- Run against DBNoiThat, the database configured by WebsiteNoiThat/Web.config.
IF OBJECT_ID(N'dbo.PasswordResetVerification', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PasswordResetVerification](
        [PasswordResetVerificationId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [UserId] INT NOT NULL,
        [DocumentNumberHash] NVARCHAR(64) NOT NULL,
        [DocumentType] NVARCHAR(20) NOT NULL,
        [FrontFileName] NVARCHAR(45) NOT NULL,
        [BackFileName] NVARCHAR(45) NULL,
        [Status] NVARCHAR(20) NOT NULL,
        [ReviewNote] NVARCHAR(500) NULL,
        [SubmittedAt] DATETIME NOT NULL,
        [ReviewedAt] DATETIME NULL,
        [ReviewedByUserId] INT NULL,
        [ResetOtpHash] NVARCHAR(64) NULL,
        [ResetOtpExpiresAt] DATETIME NULL,
        CONSTRAINT [FK_PasswordResetVerification_User]
            FOREIGN KEY ([UserId]) REFERENCES [dbo].[User] ([UserId]),
        CONSTRAINT [FK_PasswordResetVerification_Reviewer]
            FOREIGN KEY ([ReviewedByUserId]) REFERENCES [dbo].[User] ([UserId])
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.PasswordResetVerification')
      AND name = N'UX_PasswordResetVerification_UserId'
)
    CREATE UNIQUE INDEX [UX_PasswordResetVerification_UserId]
        ON [dbo].[PasswordResetVerification] ([UserId]);
GO
