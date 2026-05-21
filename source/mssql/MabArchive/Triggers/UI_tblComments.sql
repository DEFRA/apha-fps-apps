USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE trigger [dbo].[UI_tblComments] ON [dbo].[tblComments] 
FOR INSERT, UPDATE 
AS
Update tblcomments
SET DateEntered =GetDate(), MadeBy=suser_sname()
FROM tblcomments, Inserted
Where tblComments.CommentNo=Inserted.CommentNo

GO
