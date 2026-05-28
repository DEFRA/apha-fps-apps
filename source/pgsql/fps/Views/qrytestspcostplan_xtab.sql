CREATE OR REPLACE VIEW fps.qrytestspcostplan_xtab AS
 SELECT testcode,
    sum(
        CASE lower(profitcentre::text)
            WHEN 'labt'::text THEN price
            ELSE 0::money
        END) AS labt,
    sum(
        CASE lower(profitcentre::text)
            WHEN 'vsd gb'::text THEN price
            ELSE 0::money
        END) AS vetr,
    sum(
        CASE lower(profitcentre::text)
            WHEN 'viro'::text THEN price
            ELSE 0::money
        END) AS viro
   FROM fps.tbltestrccost
  GROUP BY testcode;
