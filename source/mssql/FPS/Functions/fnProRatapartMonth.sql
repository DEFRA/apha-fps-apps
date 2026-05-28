USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		IG
-- Create date: 08/11/2010
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[fnProRatapartMonth]
(
	-- Add the parameters for the function here
	@StartDate datetime, @EndDate datetime, @month int, @year int
)
RETURNS numeric(9,8)
AS
BEGIN
	-- Declare the return variable here
	DECLARE @ResultVar numeric(9,8)
	DECLARE @StartOfMonth datetime
	DECLARE @EndOfMonth datetime
	DECLARE @sd datetime
	DECLARE @ed datetime

	-- Add the T-SQL statements to compute the return value here
	Set @StartDate=ISNULL(@StartDate,'01-01-2000')
	Set @EndDate=ISNULL(@EndDate,'01-01-2200')
	Set @StartOfMonth=Convert(datetime,cast(@month as varchar(2)) + '/01/' + cast(@year as varchar(4)),101)
	Set @EndOfMonth= DateAdd(d,-1,DateAdd(m,1,@startOfMonth))

	If (@StartDate<=@StartOfMonth) and (@EndDate>=@EndOFMonth)
	-- This is a full month
		Set @ResultVar=1
	Else
	Begin
		If (@StartDate> @EndOfMonth) or (@EndDate<@StartOfMonth)
		--This is not a month to be counted.
			Set @ResultVar=0
		Else
		Begin
			If @StartDate>@StartOfMonth
				Set @sd=@StartDate	
			Else
				Set @sd=@StartOfMonth

			If @EndDate<@EndOfMonth
				Set @ed=@EndDate	
			Else
				Set @ed=@EndOfMonth
			
			Set @ResultVar= cast(DateDiff(day,@sd,@ed) as numeric(9,7))/cast(DateDiff(day,@StartOfMonth,@EndOFMonth) as numeric(9,7))

		End	
	End

	-- Return the result of the function
	RETURN @ResultVar
END


GO
