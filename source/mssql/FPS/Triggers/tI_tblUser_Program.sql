USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Trigger dbo.tI_tblUser_Program    Script Date: 3/4/00 1:48:23 PM ******/
/****** Object:  Trigger dbo.tI_tblUser_Program    Script Date: 1/11/99 3:48:25 PM ******/
CREATE trigger[dbo].[tI_tblUser_Program] on [dbo].[tblUser_Program] for INSERT as
/* ERwin Builtin Mon Jan 05 15:49:38 1998 */
/* INSERT trigger on tblUser_Program */
begin
  declare  @numrows int,
           @nullcnt int,
           @validcnt int,
           @errno   int,
           @errmsg  varchar(255)
  select @numrows = @@rowcount
  /* ERwin Builtin Mon Jan 05 15:49:38 1998 */
  /* tlkpProgram R/84 tblUser_Program ON CHILD INSERT RESTRICT */
  if
    /* %ChildFK(" or",update) */
    update(ProgramNo)
  begin
    select @nullcnt = 0
    select @validcnt = count(*)
      from inserted,tlkpProgram
        where
          /* %JoinFKPK(inserted,tlkpProgram) */
          inserted.ProgramNo = tlkpProgram.ProgramNo
    /* %NotnullFK(inserted," is null","select @nullcnt = count(*) from inserted where"," and") */
    
    if @validcnt + @nullcnt != @numrows
    begin
      select @errno  = 30002,
             @errmsg = 'Cannot INSERT "tblUser_Program" because "tlkpProgram" does not exist.'
      goto error
    end
  end
  /* ERwin Builtin Mon Jan 05 15:49:38 1998 */
  /* tblUsers R/82 tblUser_Program ON CHILD INSERT RESTRICT */
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
             @errmsg = 'Cannot INSERT "tblUser_Program" because "tblUsers" does not exist.'
      goto error
    end
  end
  /* ERwin Builtin Mon Jan 05 15:49:38 1998 */
  return
error:
    raiserror (@errmsg, @errno, 1)
    rollback transaction
end


GO
