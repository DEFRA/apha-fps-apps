USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MY_WorkGroupGrade](
    [Year] [int] NOT NULL,
    [WGGrade] [varchar](50) NOT NULL,
    [ProfitCentreGrade] [varchar](20) NOT NULL,
    [GradeCode] [varchar](10) NOT NULL,
    [WorkGroup] [varchar](50) NOT NULL
,    CONSTRAINT [PK__MY_WorkGroupGrade__2DE6D218] PRIMARY KEY CLUSTERED
    (
        Year, WGGrade
    )
) ON [PRIMARY]
GO
