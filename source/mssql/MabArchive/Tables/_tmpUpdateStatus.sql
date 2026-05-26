USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[_tmpUpdateStatus](
    [ParentProject] [varchar](20) NOT NULL,
    [FPSStatus] [varchar](50) NULL,
    [MAStatus] [varchar](50) NULL,
    [Year] [smallint] NULL
) ON [PRIMARY]
GO
