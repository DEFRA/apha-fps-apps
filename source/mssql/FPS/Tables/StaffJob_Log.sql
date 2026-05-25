USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StaffJob_Log](
    [SequenceNo] [int] IDENTITY(1,1) NOT NULL,
    [StaffID] [varchar](50) NOT NULL,
    [Jobcode] [varchar](20) NOT NULL,
    [plannedhours] [float] NOT NULL,
    [Date_Time] [datetime] NULL,
    [User_ID] [varchar](20) NULL,
    [Insert_Delete] [char](2) NULL
,    CONSTRAINT [PK_StaffJob_Log] PRIMARY KEY NONCLUSTERED
    (
        SequenceNo
    )
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [Ind_Dt] ON [dbo].[StaffJob_Log]
(
    Date_Time
)
GO
CREATE NONCLUSTERED INDEX [Ind_JC] ON [dbo].[StaffJob_Log]
(
    Jobcode
)
GO
