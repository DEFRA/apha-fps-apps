USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vtblContract]
AS
SELECT *
FROM dbo.tblContract
WHERE (Category IN
        (SELECT tblUser_Category.Category
      FROM tblUser_Category
      WHERE tblUser_Category.User_ID IN
               (SELECT tblUsers.User_ID
             FROM tblUsers
             WHERE tblUsers.DT2UserName = USER_NAME())))
WITH CHECK OPTION

GO
