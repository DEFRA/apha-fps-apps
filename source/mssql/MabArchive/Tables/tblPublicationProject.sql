USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblPublicationProject](
    [PublicationUID] [int] NOT NULL,
    [ParentProject] [varchar](20) NOT NULL
,    CONSTRAINT [PK_tblPublicationProject] PRIMARY KEY CLUSTERED
    (
        PublicationUID, ParentProject
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tblPublicationProject] WITH CHECK ADD CONSTRAINT [FK_tblPublicationProject_tblPublication] FOREIGN KEY(PublicationUID)
REFERENCES [dbo].[tblPublication] (UID)
GO
ALTER TABLE [dbo].[tblPublicationProject] CHECK CONSTRAINT [FK_tblPublicationProject_tblPublication]
GO
