USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[temptblProjectYear](
    [Project] [int] NOT NULL CONSTRAINT [DF__Temporary__Proje__5165187F] DEFAULT (0),
    [YearNo] [int] NOT NULL
,    CONSTRAINT [aaaaatemptblProjectYear_PK] PRIMARY KEY NONCLUSTERED
    (
        Project, YearNo
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[temptblProjectYear] WITH CHECK ADD CONSTRAINT [temptblProjectYear_FK00] FOREIGN KEY(Project)
REFERENCES [dbo].[temptblProject] (Project)
GO
ALTER TABLE [dbo].[temptblProjectYear] CHECK CONSTRAINT [temptblProjectYear_FK00]
GO
CREATE NONCLUSTERED INDEX [temptblProjecttemptblProjectYear] ON [dbo].[temptblProjectYear]
(
    Project
)
GO
