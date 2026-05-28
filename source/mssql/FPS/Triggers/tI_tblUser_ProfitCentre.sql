USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Trigger dbo.tI_tblUser_ProfitCentre    Script Date: 3/4/00 1:48:24 PM ******/
/****** Object:  Trigger dbo.tI_tblUser_ProfitCentre    Script Date: 1/11/99 3:48:25 PM ******/
CREATE trigger[dbo].[tI_tblUser_ProfitCentre] on [dbo].[tblUser_ProfitCentre] for INSERT as
/* ERwin Builtin Wed Jan 07 13:41:00 1998 */
/* INSERT trigger on tblUser_ProfitCentre */
begin
  declare  @numrows int,
           @nullcnt int,
           @validcnt int,
           @errno   int,
           @errmsg  varchar(255)
  select @numrows = @@rowcount
  /* ERwin Builtin Wed Jan 07 13:41:00 1998 */
  /* tblkpProfitCentre R/90 tblUser_ProfitCentre ON CHILD INSERT RESTRICT */
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
             @errmsg = 'Cannot INSERT "tblUser_ProfitCentre" because "tblkpProfitCentre" does not exist.'
      goto error
    end
  end
  /* ERwin Builtin Wed Jan 07 13:41:00 1998 */
  /* tblUsers R/89 tblUser_ProfitCentre ON CHILD INSERT RESTRICT */
  if
    /* %ChildFK(" or",update) */
    update(User_ID)
  begin
    select @nullcnt = 0
    select @validcnt = count(*)
      from inserted,tblUsers
        where
          /* %JoinFKPK(inserted,tblUsers) */
          inserted.User_ID = tblUsers.User_ID
    /* %NotnullFK(inserted," is null","select @nullcnt = count(*) from inserted where"," and") */
    
    if @validcnt + @nullcnt != @numrows
    begin
      select @errno  = 30002,
             @errmsg = 'Cannot INSERT "tblUser_ProfitCentre" because "tblUsers" does not exist.'
      goto error
    end
  end
  /* ERwin Builtin Wed Jan 07 13:41:00 1998 */
  return
error:
    raiserror (@errmsg, @errno, 1)
    rollback transaction
end


GO
