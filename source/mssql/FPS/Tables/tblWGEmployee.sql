USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblWGEmployee](
    [PACTid] [varchar](50) NOT NULL,
    [SPNumber] [varchar](10) NOT NULL,
    [WorkGroupGrade] [varchar](50) NOT NULL,
    [PersonStatus] [varchar](10) NOT NULL CONSTRAINT [DF_tblWGEmplo_PersonStatu1__20] DEFAULT ('A'),
    [PersonClass] [varchar](10) NULL,
    [HrsPaid] [float] NOT NULL,
    [Leave] [float] NOT NULL,
    [SickSpecial] [float] NOT NULL,
    [HrsAvail] [float] NOT NULL,
    [MakeAvailable] [int] NOT NULL CONSTRAINT [DF_tblWGEmplo_MakeAvailab2__20] DEFAULT ((-1)),
    [TimeRecorder] [int] NOT NULL CONSTRAINT [DF_tblWGEmployee_TimeRecorder] DEFAULT ((0)),
    [StartDate] [datetime] NULL,
    [EndDate] [datetime] NULL,
    [HoursPerWeek] [float] NULL
,    CONSTRAINT [PK_tblWGEmployee_1__10] PRIMARY KEY NONCLUSTERED
    (
        PACTid
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tblWGEmployee] WITH CHECK ADD CONSTRAINT [FK_tblWGEmployee_2__10] FOREIGN KEY(SPNumber)
REFERENCES [dbo].[tblEmployee] (SPNumber)
GO
ALTER TABLE [dbo].[tblWGEmployee] CHECK CONSTRAINT [FK_tblWGEmployee_2__10]
GO
ALTER TABLE [dbo].[tblWGEmployee] WITH CHECK ADD CONSTRAINT [FK_tblWGEmployee_3__10] FOREIGN KEY(WorkGroupGrade)
REFERENCES [dbo].[WorkGroupGrade] (WGGrade)
GO
ALTER TABLE [dbo].[tblWGEmployee] CHECK CONSTRAINT [FK_tblWGEmployee_3__10]
GO
CREATE NONCLUSTERED INDEX [IX_tblWGEmployee_MakeAvailable] ON [dbo].[tblWGEmployee]
(
    MakeAvailable, PACTid, SPNumber, WorkGroupGrade
)
INCLUDE (PACTid, SPNumber, WorkGroupGrade)
GO
