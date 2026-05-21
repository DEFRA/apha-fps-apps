CREATE OR REPLACE VIEW mabarchive.vasuprojectlist AS
 SELECT my_tlkpproject_all.year,
    my_tlkpproject_all.parentproject,
    my_tlkpproject_all.program,
    g_tlkpproject.projecttitle,
    my_tlkpproject_all.isdefraproject
   FROM mabarchive.my_tlkpproject_all
     LEFT JOIN mabarchive.g_tlkpproject ON my_tlkpproject_all.parentproject::text = g_tlkpproject.parentproject::text
  WHERE (EXTRACT(month FROM CURRENT_DATE) = ANY (ARRAY[1::numeric, 2::numeric, 3::numeric])) AND my_tlkpproject_all.year::numeric >= (EXTRACT(year FROM CURRENT_DATE) - 1::numeric) OR (EXTRACT(month FROM CURRENT_DATE) = ANY (ARRAY[4::numeric, 5::numeric, 6::numeric, 7::numeric, 8::numeric, 9::numeric, 10::numeric, 11::numeric, 12::numeric])) AND my_tlkpproject_all.year::numeric >= EXTRACT(year FROM CURRENT_DATE);
