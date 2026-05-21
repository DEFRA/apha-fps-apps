USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/****** Object:  Trigger dbo.tU_tblkpProfitCentre    Script Date: 3/4/00 1:48:24 PM ******/
/****** Object:  Trigger dbo.tU_tblkpProfitCentre    Script Date: 1/11/99 3:48:25 PM ******/
CREATE trigger[dbo].[tU_tblkpProfitCentre] on [dbo].[tblkpProfitCentre]
  for UPDATE
  as
/* ERwin Builtin Wed Jan 07 14:17:17 1998 */
/* UPDATE trigger on tblkpProfitCentre */
begin
  declare  @numrows int,
           @nullcnt int,
           @validcnt int,
   @insProfitCentre varchar(50),
           @errno   int,
           @errmsg  varchar(255)
  select @numrows = @@rowcount

  /* ERwin Builtin Wed Jan 07 14:17:17 1998 */
  /* tblkpProfitCentre R/90 tblUser_ProfitCentre ON PARENT UPDATE CASCADE */
 if
    /* %ParentPK(" or",update) */
    update(ProfitCentre)
  begin
    if @numrows = 1
    begin
      select @insProfitCentre = inserted.ProfitCentre
        from inserted
      update tblUser_ProfitCentre
      set
        /*  %JoinFKPK(tblUser_ProfitCentre,@ins," = ",",") */
        tblUser_ProfitCentre.ProfitCentre = @insProfitCentre
      from tblUser_ProfitCentre,inserted,deleted
      where
        /*  %JoinFKPK(tblUser_ProfitCentre,deleted," = "," and") */
        tblUser_ProfitCentre.ProfitCentre = deleted.ProfitCentre
    end
    else
    begin
      select @errno = 30006,
             @errmsg = 'Cannot cascade "tblkpProfitCentre" UPDATE because more than one row has been affected.'
      goto error
    end
  end
/* ERwin Builtin Wed Jan 07 14:17:17 1998 */
  /* tblkpProfitCentre R/44 ProfitCentreGrade ON PARENT UPDATE CASCADE */
  if
    /* %ParentPK(" or",update) */
    update(ProfitCentre)
  begin
    if @numrows = 1
    begin
      select @insProfitCentre = inserted.ProfitCentre
        from inserted
      update ProfitCentreGrade
      set
        /*  %JoinFKPK(ProfitCentreGrade,@ins," = ",",") */
        ProfitCentreGrade.ProfitCentre = @insProfitCentre
      from ProfitCentreGrade,inserted,deleted
      where
        /*  %JoinFKPK(ProfitCentreGrade,deleted," = "," and") */
        ProfitCentreGrade.ProfitCentre = deleted.ProfitCentre
    end
    else
    begin
      select @errno = 30006,
             @errmsg = 'Cannot cascade "tblkpProfitCentre" UPDATE because more than one row has been affected.'
      goto error
    end
  end
  /* ERwin Builtin Wed Jan 07 14:17:17 1998 */
  /* tblkpProfitCentre R/39 WorkGroup ON PARENT UPDATE CASCADE */
  if
    /* %ParentPK(" or",update) */
    update(ProfitCentre)
  begin
    if @numrows = 1
    begin
      select @insProfitCentre = inserted.ProfitCentre
        from inserted
      update WorkGroup
      set
        /*  %JoinFKPK(WorkGroup,@ins," = ",",") */
        WorkGroup.ProfitCentre = @insProfitCentre
      from WorkGroup,inserted,deleted
      where
        /*  %JoinFKPK(WorkGroup,deleted," = "," and") */
        WorkGroup.ProfitCentre = deleted.ProfitCentre
    end
    else
    begin
      select @errno = 30006,
             @errmsg = 'Cannot cascade "tblkpProfitCentre" UPDATE because more than one row has been affected.'
      goto error
    end
  end
  /* ERwin Builtin Wed Jan 07 14:17:17 1998 */

  /* IG added 07/11/2005 */
    If (select count(*) from inserted) !=(select count(*) from inserted where inserted.profitcentre IN(SELECT tblUser_ProfitCentre.Profitcentre FROM tblUser_ProfitCentre 
	WHERE tblUser_ProfitCentre.User_ID IN (SELECT tblUsers.User_ID FROM tblUsers 
	WHERE tblUsers.UserName = USER_NAME())))
    begin
      select @errno = 30006,
             @errmsg = 'You do not have permission to update all of these Profit Centres.'
      goto error
    end

  return
error:
    raiserror (@errmsg, @errno, 1)
    rollback transaction
end




GO
