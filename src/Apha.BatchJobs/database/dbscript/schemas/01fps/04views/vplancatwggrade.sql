-- View: fps.vplancatwggrade

CREATE OR REPLACE VIEW fps.vplancatwggrade AS
 SELECT plancategory,
    wggrade,
    hours,
    createdby,
    selleragrees,
    buyeragrees
   FROM fps.plancatwggrade
  WHERE ((wggrade)::text IN ( SELECT vworkgroupgrade.wggrade
           FROM fps.vworkgroupgrade));
