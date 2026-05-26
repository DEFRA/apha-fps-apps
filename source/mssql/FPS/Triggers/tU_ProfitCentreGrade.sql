USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  Trigger dbo.tU_ProfitCentreGrade    Script Date: 3/4/00 1:48:24 PM ******/
/****** Object:  Trigger dbo.tU_ProfitCentreGrade    Script Date: 1/11/99 3:48:25 PM ******/
CREATE trigger[dbo].[tU_ProfitCentreGrade] on [dbo].[ProfitCentreGrade] for UPDATE as
/* ERwin Builtin Wed Jan 07 14:17:17 1998 */
/* UPDATE trigger on ProfitCentreGrade */
begin
  declare  @numrows int,
           @nullcnt int,
           @validcnt int,
           @insPCGrade varchar(20),
           @errno   int,
           @errmsg  varchar(255)
  select @numrows = @@rowcount
  /* ERwin Builtin Wed Jan 07 14:17:17 1998 */
  /* tblkpProfitCentre R/44 ProfitCentreGrade ON CHILD UPDATE RESTRICT */
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
      select @errno  = 30007,
             @errmsg = 'Cannot UPDATE "ProfitCentreGrade" because "tblkpProfitCentre" does not exist.'
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
