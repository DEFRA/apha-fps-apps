USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Trigger dbo.tI_WorkGroup    Script Date: 3/4/00 1:48:24 PM ******/
/****** Object:  Trigger dbo.tI_WorkGroup    Script Date: 1/11/99 3:48:25 PM ******/
CREATE trigger[dbo].[tI_WorkGroup] on [dbo].[WorkGroup]
  for INSERT
  as
/* ERwin Builtin Wed Jan 07 14:17:17 1998 */
/* INSERT trigger on WorkGroup */
begin
  declare  @numrows int,
           @nullcnt int,
           @validcnt int,
           @errno   int,
     @errmsg  varchar(255)
  select @numrows = @@rowcount
  /* ERwin Builtin Wed Jan 07 14:17:17 1998 */
  /* tblkpProfitCentre R/39 WorkGroup ON CHILD INSERT RESTRICT */
  if
    /* %ChildFK(" or",update) */
    update(ProfitCentre)
  begin
 select @nullcnt = 0
    select @validcnt = count(*)
      from inserted,tblkpProfitCentre
        where
          /* %JoinFKPK(inserted,tblkpProfitCentre) */
          inserted.ProfitCentre = tblkpProfitCentre.ProfitCentre
    /* %NotnullFK(inserted," is null","select @nullcnt = count(*) from inserted where"," and") */
    
    if @validcnt + @nullcnt != @numrows
    begin
      select @errno  = 30002,
             @errmsg = 'Cannot INSERT "WorkGroup" because "tblkpProfitCentre" does not exist.'
      goto error
    end
  end
  /* ERwin Builtin Wed Jan 07 14:17:17 1998 */
  return
error:
    raiserror (@errmsg, @errno, 1)
    rollback transaction
end


GO
