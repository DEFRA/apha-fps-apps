-- Table: fps.tblcurrentmonth

CREATE TABLE fps.tblcurrentmonth (
    currentmonth integer
);

COMMENT ON TABLE fps.tblcurrentmonth IS $$This table stores the current month value.$$;

COMMENT ON COLUMN fps.tblcurrentmonth.currentmonth IS $$Stores the current month as an integer value.$$;
