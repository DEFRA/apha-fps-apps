USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblProjectYear](
    [Project] [varchar](50) NOT NULL,
    [YearNo] [int] NOT NULL,
    [Markup_Time] [float] NULL,
    [Markup_Tests] [float] NULL,
    [Markup_Animals] [float] NULL,
    [Markup_Additional] [float] NULL,
    [Profit_Time] [float] NULL,
    [Profit_Tests] [float] NULL,
    [Profit_Animals] [float] NULL,
    [Profit_Additional] [float] NULL
,    CONSTRAINT [aaaaatblProjectYear_PK] PRIMARY KEY NONCLUSTERED
    (
        Project, YearNo
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tblProjectYear] WITH CHECK ADD CONSTRAINT [tblProjectYear_FK00] FOREIGN KEY(Project)
REFERENCES [dbo].[tblProject] (Project)
GO
ALTER TABLE [dbo].[tblProjectYear] CHECK CONSTRAINT [tblProjectYear_FK00]
GO
CREATE NONCLUSTERED INDEX [tblProjecttblProjectYear] ON [dbo].[tblProjectYear]
(
    Project
)
GO
