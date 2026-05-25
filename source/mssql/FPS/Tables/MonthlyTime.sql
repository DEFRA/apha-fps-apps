USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MonthlyTime](
    [PACTStaffID] [varchar](50) NOT NULL,
    [TimeCode] [varchar](50) NOT NULL,
    [Month] [float] NOT NULL,
    [ParentProject] [varchar](20) NOT NULL,
    [WorkGroup] [varchar](50) NULL,
    [Hours] [float] NULL
,    CONSTRAINT [PK_MonthlyTime] PRIMARY KEY CLUSTERED
    (
        PACTStaffID, TimeCode, Month, ParentProject
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[MonthlyTime] WITH CHECK ADD CONSTRAINT [FK_MonthlyTime_2__10] FOREIGN KEY(WorkGroup, TimeCode, ParentProject)
REFERENCES [dbo].[TimeCodeValid] (WorkGroup, TimeCode, ParentProject)
GO
ALTER TABLE [dbo].[MonthlyTime] CHECK CONSTRAINT [FK_MonthlyTime_2__10]
GO
ALTER TABLE [dbo].[MonthlyTime] WITH CHECK ADD CONSTRAINT [FK_MonthlyTime_3__10] FOREIGN KEY(PACTStaffID)
REFERENCES [dbo].[tblWGEmployee] (PACTid)
GO
ALTER TABLE [dbo].[MonthlyTime] CHECK CONSTRAINT [FK_MonthlyTime_3__10]
GO
CREATE NONCLUSTERED INDEX [ijnd_StaffID] ON [dbo].[MonthlyTime]
(
    PACTStaffID
)
GO
CREATE NONCLUSTERED INDEX [Reference23] ON [dbo].[MonthlyTime]
(
    WorkGroup, TimeCode, ParentProject
)
GO
CREATE NONCLUSTERED INDEX [TimeCode] ON [dbo].[MonthlyTime]
(
    TimeCode
)
GO
CREATE NONCLUSTERED INDEX [WorkGroup] ON [dbo].[MonthlyTime]
(
    WorkGroup
)
GO
