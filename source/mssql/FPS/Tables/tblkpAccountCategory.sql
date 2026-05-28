USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblkpAccountCategory](
    [AccShortName] [varchar](50) NOT NULL,
    [AccountDescription] [varchar](50) NULL,
    [ConstituentAccountCodes] [varchar](100) NULL,
    [AccountType] [varchar](10) NOT NULL,
    [ProjectSpecific] [int] NULL,
    [RCSpecific] [int] NULL,
    [CSG7_Group] [char](15) NULL
,    CONSTRAINT [PK__tblkpAccountCate__02DC7882] PRIMARY KEY CLUSTERED
    (
        AccShortName
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tblkpAccountCategory] WITH CHECK ADD CONSTRAINT [CK__tblkpAcco__Accou__01E85449] CHECK ([AccountType]='Pay' OR [AccountType]='NPRC')
GO
CREATE NONCLUSTERED INDEX [AccountType] ON [dbo].[tblkpAccountCategory]
(
    AccountType
)
GO
