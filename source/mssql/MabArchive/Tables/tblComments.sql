USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblComments](
    [CommentNo] [int] IDENTITY(1,1) NOT NULL,
    [Project] [varchar](20) NOT NULL,
    [Year] [smallint] NOT NULL,
    [DateEntered] [datetime] NULL,
    [Topic] [varchar](25) NOT NULL,
    [Comment] [text] NULL,
    [MadeBy] [char](20) NULL
,    CONSTRAINT [IX_tblComments] UNIQUE NONCLUSTERED
    (
        Project, Year, Topic
    )
,    CONSTRAINT [PK_tblComments] PRIMARY KEY NONCLUSTERED
    (
        CommentNo
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tblComments] WITH CHECK ADD CONSTRAINT [FK_tblComments_tlkpCommentTopics] FOREIGN KEY(Topic)
REFERENCES [dbo].[tlkpCommentTopics] (Topic)
GO
ALTER TABLE [dbo].[tblComments] CHECK CONSTRAINT [FK_tblComments_tlkpCommentTopics]
GO
