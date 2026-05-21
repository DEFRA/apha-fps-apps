USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MY_tblStaffJob](
    [Year] [smallint] NOT NULL,
    [StaffID] [varchar](50) NOT NULL,
    [Jobcode] [varchar](20) NOT NULL,
    [plannedhours] [float] NOT NULL,
    [SysTimeStamp] [binary](8) NULL
,    CONSTRAINT [PK_MY_tblStaffJob] PRIMARY KEY CLUSTERED
    (
        Year, StaffID, Jobcode
    )
) ON [PRIMARY]
GO
