USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[usp_CostCentreWGs] 
as
begin
select
	CostCentre,
	ProfitCentre,
	WGs=left(o.list,len(o.list)-1)

from 
	workgroup wg
cross apply
(
	select Workgroup + ', '
	from workgroup
	where workgroup.costcentre=wg.costcentre
	for XML PATH('')
) o (list)
group by 
	ProfitCentre,
	o.list,
	CostCentre
having   costcentre is not null
end

GO
