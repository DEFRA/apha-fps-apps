USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Trigger dbo.tblUsers_UTrig    Script Date: 3/4/00 1:48:23 PM ******/
/****** Object:  Trigger dbo.tblUsers_UTrig    Script Date: 1/11/99 3:48:25 PM ******/
CREATE trigger[dbo].[tblUsers_UTrig] on [dbo].[tblUsers]
  for UPDATE
  as
/* ERwin Builtin Mon Jan 05 17:39:23 1998 */
/* UPDATE trigger on tblUsers */
/* default body for tblUsers_UTrig */
begin
  declare  @numrows int,
           @nullcnt int,
           @validcnt int,
           @insUser_ID int,
           @errno   int,
           @errmsg  varchar(255)
  select @numrows = @@rowcount
/* ERwin Builtin Mon Jan 05 17:39:23 1998 */
  /* tblUsers R/82 tblUser_Program ON PARENT UPDATE CASCADE */
  if
   /* %ParentPK(" or",update) */
    update(User_ID)
  begin
    if @numrows = 1
    begin
      select @insUser_ID = inserted.User_ID
        from inserted
      update tblUser_Program
      set
        /*  %JoinFKPK(tblUser_Program,@ins," = ",",") */
        tblUser_Program.User_ID = @insUser_ID
      from tblUser_Program,inserted,deleted
      where
        /*  %JoinFKPK(tblUser_Program,deleted," = "," and") */
        tblUser_Program.User_ID = deleted.User_ID
    end
    else
    begin
      select @errno = 30006,
             @errmsg = 'Cannot cascade "tblUsers" UPDATE because more than one row has been affected.'
      goto error
    end
  end
  return
error:
    raiserror (@errmsg, @errno, 1)
    rollback transaction
end


GO
