USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[temptblTestReq](
    [Project] [int] NOT NULL CONSTRAINT [DF__Temporary__Proje__6FE99F9F] DEFAULT (0),
    [Year] [int] NOT NULL CONSTRAINT [DF__TemporaryU__Year__70DDC3D8] DEFAULT (0),
    [TestCode] [nvarchar](50) NOT NULL,
    [NoTests] [float] NULL CONSTRAINT [DF__Temporary__NoTes__71D1E811] DEFAULT (0),
    [UnitPrice] [float] NULL CONSTRAINT [DF__Temporary__UnitP__72C60C4A] DEFAULT (0)
,    CONSTRAINT [aaaaatemptblTestReq_PK] PRIMARY KEY NONCLUSTERED
    (
        Project, Year, TestCode
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[temptblTestReq] WITH CHECK ADD CONSTRAINT [temptblTestReq_FK00] FOREIGN KEY(Year, Project)
REFERENCES [dbo].[temptblProjectYear] (YearNo, Project)
GO
ALTER TABLE [dbo].[temptblTestReq] CHECK CONSTRAINT [temptblTestReq_FK00]
GO
CREATE NONCLUSTERED INDEX [tblTestRequProject] ON [dbo].[temptblTestReq]
(
    Project
)
GO
CREATE NONCLUSTERED INDEX [temptblProjectYeartemptblTestReq] ON [dbo].[temptblTestReq]
(
    Project, Year
)
GO
