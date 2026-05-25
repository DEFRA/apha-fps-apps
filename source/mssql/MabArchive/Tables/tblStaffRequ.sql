USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblStaffRequ](
    [SR_Identity] [int] IDENTITY(1,1) NOT NULL,
    [Project] [varchar](50) NULL,
    [Year] [int] NULL CONSTRAINT [DF__TemporaryU__Year__37A5467C] DEFAULT (0),
    [WGGrade] [nvarchar](20) NOT NULL,
    [Name] [nvarchar](50) NULL,
    [NoHours] [float] NULL CONSTRAINT [DF__Temporary__NoHou__38996AB5] DEFAULT (0),
    [NoDays] [float] NULL CONSTRAINT [DF__Temporary__NoDay__398D8EEE] DEFAULT (0),
    [ChargeRate] [float] NULL CONSTRAINT [DF__Temporary__Charg__3A81B327] DEFAULT (0),
    [PayRate] [float] NULL,
    [NPR] [float] NULL,
    [OHR] [float] NULL
,    CONSTRAINT [aaaaatblStaffRequ_PK] PRIMARY KEY NONCLUSTERED
    (
        SR_Identity
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tblStaffRequ] WITH CHECK ADD CONSTRAINT [tblStaffRequ_FK00] FOREIGN KEY(Year, Project)
REFERENCES [dbo].[tblProjectYear] (YearNo, Project)
GO
ALTER TABLE [dbo].[tblStaffRequ] CHECK CONSTRAINT [tblStaffRequ_FK00]
GO
CREATE NONCLUSTERED INDEX [tblProjectYeartblStaffRequ] ON [dbo].[tblStaffRequ]
(
    Project, Year
)
GO
CREATE NONCLUSTERED INDEX [tblStaffRequProject] ON [dbo].[tblStaffRequ]
(
    Project
)
GO
