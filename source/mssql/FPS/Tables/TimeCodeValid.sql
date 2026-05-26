USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TimeCodeValid](
    [TimeCode] [varchar](50) NOT NULL,
    [WorkGroup] [varchar](50) NOT NULL,
    [ParentProject] [varchar](20) NOT NULL,
    [TestCode] [varchar](50) NULL,
    [JobCode] [varchar](50) NULL,
    [Portfolio] [varchar](20) NULL,
    [Active] [bit] NOT NULL
,    CONSTRAINT [aaaaaTimeCodeValid_PK] PRIMARY KEY NONCLUSTERED
    (
        WorkGroup, TimeCode, ParentProject
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[TimeCodeValid] WITH CHECK ADD CONSTRAINT [FK_TimeCodeValid_1__11] FOREIGN KEY(ParentProject)
REFERENCES [dbo].[tlkpProject] (ParentProject)
GO
ALTER TABLE [dbo].[TimeCodeValid] CHECK CONSTRAINT [FK_TimeCodeValid_1__11]
GO
CREATE NONCLUSTERED INDEX [Reference20] ON [dbo].[TimeCodeValid]
(
    JobCode
)
GO
CREATE NONCLUSTERED INDEX [Reference24] ON [dbo].[TimeCodeValid]
(
    TestCode, Portfolio
)
GO
CREATE NONCLUSTERED INDEX [Reference3] ON [dbo].[TimeCodeValid]
(
    ParentProject
)
GO
