USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MT_Log](
    [SequenceNo] [int] IDENTITY(1,1) NOT NULL,
    [PACTStaffID] [varchar](50) NOT NULL,
    [TimeCode] [varchar](50) NOT NULL,
    [Month] [float] NOT NULL,
    [ParentProject] [varchar](20) NOT NULL,
    [WorkGroup] [varchar](50) NULL,
    [Hours] [float] NULL,
    [Date_Time] [datetime] NULL,
    [User_ID] [varchar](20) NULL,
    [Insert_Delete] [char](2) NULL
) ON [PRIMARY]
GO
