USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[G_tlkpProject](
    [ParentProject] [varchar](20) NOT NULL,
    [ProjectTitle] [varchar](200) NULL,
    [CostBookNo] [varchar](50) NULL,
    [Disease] [varchar](50) NULL,
    [Contract] [varchar](10) NULL,
    [ShortTitle] [varchar](30) NULL,
    [ProjectStatus] [varchar](50) NULL
,    CONSTRAINT [PK_G_tlkpProject] PRIMARY KEY CLUSTERED
    (
        ParentProject
    )
) ON [PRIMARY]
GO
