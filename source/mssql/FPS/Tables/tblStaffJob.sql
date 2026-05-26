USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblStaffJob](
    [StaffID] [varchar](50) NOT NULL,
    [Jobcode] [varchar](20) NOT NULL,
    [plannedhours] [float] NOT NULL CONSTRAINT [DF__tblStaffJ__plann__623A9EC6] DEFAULT (0),
    [SysTimeStamp] [timestamp] NULL
,    CONSTRAINT [PK__tblStaffJob__30392EDE] PRIMARY KEY CLUSTERED
    (
        StaffID, Jobcode
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tblStaffJob] WITH CHECK ADD CONSTRAINT [FK__tblStaffJ__JobCo__723BFC65] FOREIGN KEY(Jobcode)
REFERENCES [dbo].[tlkpProject] (ParentProject)
GO
ALTER TABLE [dbo].[tblStaffJob] CHECK CONSTRAINT [FK__tblStaffJ__JobCo__723BFC65]
GO
ALTER TABLE [dbo].[tblStaffJob] WITH CHECK ADD CONSTRAINT [FK_tblStaffJob_1__10] FOREIGN KEY(StaffID)
REFERENCES [dbo].[tblWGEmployee] (PACTid)
GO
ALTER TABLE [dbo].[tblStaffJob] CHECK CONSTRAINT [FK_tblStaffJob_1__10]
GO
