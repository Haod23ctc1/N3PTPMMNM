-- Run against DBNoiThat, the database configured by WebsiteNoiThat/Web.config.
IF OBJECT_ID(N'dbo.IdentityVerification', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[IdentityVerification](
        [IdentityVerificationId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [UserId] INT NOT NULL,
        [DocumentType] NVARCHAR(20) NOT NULL,
        [DocumentNumberHash] NVARCHAR(64) NOT NULL,
        [FrontFileName] NVARCHAR(45) NULL,
        [BackFileName] NVARCHAR(45) NULL,
        [Status] NVARCHAR(20) NOT NULL,
        [ReviewNote] NVARCHAR(500) NULL,
        [SubmittedAt] DATETIME NOT NULL,
        [ReviewedAt] DATETIME NULL,
        [ReviewedByUserId] INT NULL,
        CONSTRAINT [FK_IdentityVerification_User]
            FOREIGN KEY ([UserId]) REFERENCES [dbo].[User] ([UserId]),
        CONSTRAINT [FK_IdentityVerification_Reviewer]
            FOREIGN KEY ([ReviewedByUserId]) REFERENCES [dbo].[User] ([UserId])
    );
END
GO

IF COL_LENGTH(N'dbo.IdentityVerification', N'DocumentNumberHash') IS NULL
BEGIN
    ALTER TABLE [dbo].[IdentityVerification]
        ADD [DocumentNumberHash] NVARCHAR(64) NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.IdentityVerification')
      AND name = N'UX_IdentityVerification_UserId'
)
    CREATE UNIQUE INDEX [UX_IdentityVerification_UserId]
        ON [dbo].[IdentityVerification] ([UserId]);
GO
