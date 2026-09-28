--liquibase formatted sql

--changeset repo-admin:CR033 labels:dml context:all

INSERT INTO fps.tblyearmaster
(
    fpsyear,
    fpsyearcode,
    yearstatus,
    remarks,
    active,
    createdon,
    createdby
)
SELECT *
FROM (
    VALUES
        (2016,'2016-17','Closed','year 2016',true,now(),'SP'),
        (2017,'2017-18','Closed','year 2017',true,now(),'SP'),
        (2018,'2018-19','Closed','year 2018',true,now(),'SP'),
        (2019,'2019-20','Closed','year 2019',true,now(),'SP'),
        (2020,'2020-21','Closed','year 2020',true,now(),'SP'),
        (2021,'2021-22','Closed','year 2021',true,now(),'SP'),
        (2022,'2022-23','Closed','year 2022',true,now(),'SP'),
        (2023,'2023-24','Closed','year 2023',true,now(),'SP'),
        (2024,'2024-25','Closed','year 2024',true,now(),'SP'),
        (2025,'2025-26','Open','year 2025',true,now(),'SP'),
        (2026,'2026-27','Planned','year 2026',true,now(),'SP')
) AS v
(
    fpsyear,
    fpsyearcode,
    yearstatus,
    remarks,
    active,
    createdon,
    createdby
)
WHERE NOT EXISTS (
    SELECT 1
    FROM fps.tblyearmaster t
    WHERE t.fpsyear = v.fpsyear
);
--ROLLBACK
--Not Applicable