USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblTestRequ](
    [Project] [varchar](50) NOT NULL,
    [Year] [int] NOT NULL CONSTRAINT [DF__TemporaryU__Year__3F466844] DEFAULT (0),
    [TestCode] [nvarchar](50) NOT NULL,
    [NoTests] [float] NULL CONSTRAINT [DF__Temporary__NoTes__403A8C7D] DEFAULT (0),
    [UnitPrice] [float] NULL CONSTRAINT [DF__Temporary__UnitP__412EB0B6] DEFAULT (0)
,    CONSTRAINT [PK_tblTestRequ] PRIMARY KEY NONCLUSTERED
    (
        Project, Year, TestCode
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tblTestRequ] WITH CHECK ADD CONSTRAINT [tblTestRequ_FK00] FOREIGN KEY(Year, Project)
REFERENCES [dbo].[tblProjectYear] (YearNo, Project)
GO
ALTER TABLE [dbo].[tblTestRequ] CHECK CONSTRAINT [tblTestRequ_FK00]
GO
CREATE NONCLUSTERED INDEX [tblProjectYeartblTestRequ] ON [dbo].[tblTestRequ]
(
    Project, Year
)
GO
CREATE NONCLUSTERED INDEX [tblTestRequProject] ON [dbo].[tblTestRequ]
(
    Project
)
GO
