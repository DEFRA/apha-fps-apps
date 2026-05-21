USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblReportGroup_Link](
    [ReportID] [int] NOT NULL,
    [GroupID] [int] NOT NULL
,    CONSTRAINT [PK_tblReportGroup_Link] PRIMARY KEY CLUSTERED
    (
        ReportID, GroupID
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tblReportGroup_Link] WITH CHECK ADD CONSTRAINT [FK_tblReportGroup_Link_tblReportGroup] FOREIGN KEY(GroupID)
REFERENCES [dbo].[tblReportGroup] (GroupID)
GO
ALTER TABLE [dbo].[tblReportGroup_Link] CHECK CONSTRAINT [FK_tblReportGroup_Link_tblReportGroup]
GO
