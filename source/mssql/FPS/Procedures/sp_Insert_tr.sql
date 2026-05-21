USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  Stored Procedure dbo.sp_Insert_tr    Script Date: 3/4/00 1:48:22 PM ******/
/****** Object:  Stored Procedure dbo.sp_Insert_tr    Script Date: 7/22/99 12:07:57 PM ******/
CREATE procEDURE [dbo].[sp_Insert_tr]
@OldCode VARCHAR(20),
@NewCode VARCHAR(20) AS
DECLARE @tc VARCHAR(20)
DECLARE @buy VARCHAR(20)
DECLARE @up MONEY
DECLARE @nr FLOAT
DECLARE @pb VARCHAR(50)
DECLARE @tb VARCHAR(50)
DECLARE @dc DATETIME
DECLARE @act TINYINT
DECLARE tr_cursor INSENSITIVE CURSOR
FOR SELECT  tr.TestCode,
	@NewCode,
	tr.UnitPrice,
	tr.NoRequired,
	@NewCode,
	tr.TestBuyerCode,
	tr.DateCreated,
	tr.Active
FROM tlkpTestReqmt tr
WHERE tr.ProjectBuyerCode = @OldCode 
OPEN tr_cursor
FETCH NEXT FROM tr_cursor INTO @tc, @buy, @up, @nr, @pb, @tb, @dc, @act
WHILE (@@FETCH_STATUS <> -1)
BEGIN
	IF (@@FETCH_STATUS <> -2)
	BEGIN
		INSERT INTO tlkpTestReqmt 
			(TestCode, Buyer, UnitPrice, NoRequired, ProjectBuyerCode, TestBuyerCode, DateCreated, Active)
		VALUES( @tc, @buy, @up, @nr, @pb, @tb, @dc, @act)
	END
	FETCH NEXT FROM tr_cursor INTO @tc, @buy, @up, @nr, @pb, @tb, @dc, @act
END
DEALLOCATE tr_cursor

GO
