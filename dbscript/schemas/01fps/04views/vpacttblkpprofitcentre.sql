-- View: fps.vpacttblkpprofitcentre

CREATE OR REPLACE VIEW fps.vpacttblkpprofitcentre AS
 SELECT profitcentre,
    profitcentrename,
    division,
    conttarget,
    profitcentrehead,
    divisionid,
    email_recipient,
    pactcoordinatoremailname,
    timesheet,
    outputsheet,
    timesheetlayout
   FROM fps.tblkpprofitcentre;
