USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vTestOrProduct_TM]
AS

SELECT *
FROM TestOrProduct
WHERE (Owner IN
        (SELECT tblUser_TestOwner.Test_Owner
      FROM tblUser_TestOwner
      WHERE tblUser_TestOwner.User_ID IN
               (SELECT tblUsers.User_ID
             FROM tblUsers
             WHERE tblUsers.DT2UserName = USER_NAME())))

GO
