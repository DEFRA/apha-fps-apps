USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Trigger dbo.tD_tblkpProfitCentre    Script Date: 3/4/00 1:48:24 PM ******/
/****** Object:  Trigger dbo.tD_tblkpProfitCentre    Script Date: 1/11/99 3:48:25 PM ******/
CREATE trigger[dbo].[tD_tblkpProfitCentre] on [dbo].[tblkpProfitCentre]
  for DELETE
  as
/* ERwin Builtin Wed Jan 07 14:17:17 1998 */
/* DELETE trigger on tblkpProfitCentre */
begin
  declare  @errno   int,
           @errmsg  varchar(255)
    /* ERwin Builtin Wed Jan 07 14:17:17 1998 */
    /* tblkpProfitCentre R/90 tblUser_ProfitCentre ON PARENT DELETE CASCADE */
    delete tblUser_ProfitCentre
      from tblUser_ProfitCentre,deleted
      where
        /*  %JoinFKPK(tblUser_ProfitCentre,deleted," = "," and")*/
        tblUser_ProfitCentre.ProfitCentre = deleted.ProfitCentre
    /* ERwin Builtin Wed Jan 07 14:17:17 1998 */
    /* tblkpProfitCentre R/44 ProfitCentreGrade ON PARENT DELETE RESTRICT */
    if exists (
      select * from deleted,ProfitCentreGrade
      where
        /*  %JoinFKPK(ProfitCentreGrade,deleted," = "," and") */
        ProfitCentreGrade.ProfitCentre = deleted.ProfitCentre
    )
    begin
      select @errno  = 30001,
             @errmsg = 'Cannot DELETE "tblkpProfitCentre" because "ProfitCentreGrade" exists.'
      goto error
    end
    /* ERwin Builtin Wed Jan 07 14:17:17 1998 */
    /* tblkpProfitCentre R/39 WorkGroup ON PARENT DELETE RESTRICT */
    if exists (
      select * from deleted,WorkGroup
      where
        /*  %JoinFKPK(WorkGroup,deleted," = "," and") */
        WorkGroup.ProfitCentre = deleted.ProfitCentre
    )
    begin
      select @errno  = 30001,
             @errmsg = 'Cannot DELETE "tblkpProfitCentre" because "WorkGroup" exists.'
     goto error
    end
    /* ERwin Builtin Wed Jan 07 14:17:17 1998 */
    return
error:
    raiserror (@errmsg, @errno, 1)
    rollback transaction
end


GO
