USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tlkpCommentTopics](
    [Topic] [varchar](25) NOT NULL
,    CONSTRAINT [PK_tlkpCommentTopics] PRIMARY KEY NONCLUSTERED
    (
        Topic
    )
) ON [PRIMARY]
GO
