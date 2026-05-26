USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[temptblStaffRequ](
    [SR_Identity] [int] IDENTITY(1,1) NOT NULL,
    [Project] [int] NULL CONSTRAINT [DF__Temporary__Proje__6754599E] DEFAULT (0),
    [Year] [int] NULL CONSTRAINT [DF__TemporaryU__Year__68487DD7] DEFAULT (0),
    [WGGrade] [nvarchar](20) NULL,
    [Name] [nvarchar](50) NULL,
    [NoHours] [float] NULL CONSTRAINT [DF__Temporary__NoHou__693CA210] DEFAULT (0),
    [NoDays] [float] NULL CONSTRAINT [DF__Temporary__NoDay__6A30C649] DEFAULT (0),
    [ChargeRate] [float] NULL CONSTRAINT [DF__Temporary__Charg__6B24EA82] DEFAULT (0)
,    CONSTRAINT [aaaaatemptblStaffRequ_PK] PRIMARY KEY NONCLUSTERED
    (
        SR_Identity
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[temptblStaffRequ] WITH CHECK ADD CONSTRAINT [temptblStaffRequ_FK00] FOREIGN KEY(Project, Year)
REFERENCES [dbo].[temptblProjectYear] (Project, YearNo)
GO
ALTER TABLE [dbo].[temptblStaffRequ] CHECK CONSTRAINT [temptblStaffRequ_FK00]
GO
CREATE NONCLUSTERED INDEX [tblStaffRequProject] ON [dbo].[temptblStaffRequ]
(
    Project
)
GO
CREATE NONCLUSTERED INDEX [temptblProjectYeartemptblStaffRequ] ON [dbo].[temptblStaffRequ]
(
    Project, Year
)
GO
