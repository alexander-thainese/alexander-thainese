ALTER PROCEDURE [dbo].[CMT_GET_SCHEMA_TREE](@COUNTRY_CODE NVARCHAR(6))
AS
    BEGIN

        DECLARE @COUNTRY_ID UNIQUEIDENTIFIER

        SELECT @COUNTRY_ID = OBJECT_ID
        FROM CMT_COUNTRY
        WHERE CODE = @COUNTRY_CODE;

        WITH A
             AS (SELECT v.OBJECT_ID, V.TEXT_VALUE PARENT_NAME, V.TEXT_VALUE NAME, 4 TYPE, CAST(NULL AS UNIQUEIDENTIFIER) PARENT_ID, V.VALUE_LIST_ID AS VALUE_LIST_ID, V.CHILD_LIST_ID PARENT_LIST_ID, se.SCHEMA_ID [ROOT_ID], E.OBJECT_ID AS ELEMENT_ID, 1 [LEVEL], e.readonly, v.OBJECT_ID VALUE_ID, v.STATUS, v.GLOBAL_CODE, v.COUNTRY_ID, v.PARENT_ID PARENT_VALUE_ID
                 FROM CMT_VALUE V
                      LEFT JOIN CMT_METADATA_ELEMENT E ON E.VALUE_LIST_ID = V.VALUE_LIST_ID
                      LEFT JOIN CMT_METADATA_SCHEMA_ELEMENT SE ON SE.ELEMENT_ID = E.OBJECT_ID
                 WHERE v.EXTERNAL_ID IS NULL
                       OR v.COUNTRY_ID = @COUNTRY_ID),
             B
             AS (SELECT V.OBJECT_ID, VLH.NAME PARENT_NAME, V.TEXT_VALUE, 4 TYPE, VLH.OBJECT_ID AS ValueList_ID, VLH.VALUE_LIST_ID, V.CHILD_LIST_ID, VLH.ROOT_ID, VLH.ELEMENT_ID, VLH.LEVEL + 1 AS Level, vlh.READONLY, V.OBJECT_ID VALUE_ID, v.STATUS, v.GLOBAL_CODE, v.COUNTRY_ID, v.PARENT_ID PARENT_VALUE_ID
                 FROM A VLH
                      INNER JOIN CMT_VALUE V ON VLH.PARENT_LIST_ID = V.VALUE_LIST_ID
                 WHERE v.EXTERNAL_ID IS NULL
                       OR v.COUNTRY_ID = @COUNTRY_ID),
             VALUE_LIST_HIERARCHY
             AS (SELECT OBJECT_ID, PARENT_NAME, NAME, 4 TYPE, CAST(NULL AS UNIQUEIDENTIFIER) PARENT_ID, VALUE_LIST_ID, PARENT_LIST_ID, [ROOT_ID], ELEMENT_ID, 1 [LEVEL], readonly, VALUE_ID, STATUS, GLOBAL_CODE, COUNTRY_ID, PARENT_ID PARENT_VALUE_ID
                 FROM A
                 UNION ALL
                 SELECT OBJECT_ID, PARENT_NAME, TEXT_VALUE, 4 TYPE, ValueList_ID, VALUE_LIST_ID, CHILD_LIST_ID, ROOT_ID, ELEMENT_ID, LEVEL + 1 AS Level, READONLY, OBJECT_ID VALUE_ID, STATUS, GLOBAL_CODE, COUNTRY_ID, PARENT_VALUE_ID
                 FROM B),
             TRANSLATED_VALUES
             AS (SELECT SCHEMA_ID, ELEMENT_ID, COUNT(DISTINCT X.VALUE_ID) [ALL], SUM(CASE
                                                                                         WHEN X.VALUE_DETAIL_ID IS NOT NULL
                                                                                         THEN 1
                                                                                         ELSE 0
                                                                                     END) TRANSLATED
                 FROM
                 (
                     SELECT DISTINCT 
                            VLH.ROOT_ID SCHEMA_ID, VLH.ELEMENT_ID, v.OBJECT_ID VALUE_ID, VD.VALUE_ID VALUE_DETAIL_ID
                     FROM VALUE_LIST_HIERARCHY VLH
                          INNER JOIN CMT_METADATA_ELEMENT e ON e.OBJECT_ID = VLH.ELEMENT_ID
                          INNER JOIN CMT_METADATA_SCHEMA S ON S.OBJECT_ID = VLH.ROOT_ID --acha
                          INNER JOIN CMT_VALUE V ON V.OBJECT_ID = VLH.VALUE_ID
                                                    AND V.STATUS > 0
                          LEFT JOIN CMT_VALUE_DETAIL VD ON VD.VALUE_ID = V.OBJECT_ID
                                                           AND @COUNTRY_ID = VD.COUNTRY_ID
                     WHERE e.STATUS > 0 --acha
                 ) X
                 GROUP BY ROLLUP(X.schema_id, X.ELEMENT_ID)
                 HAVING ELEMENT_ID IS NOT NULL)
             SELECT NULL UNIQUE_ID, D.*, ISNULL(v.TEXT_VALUE, d.DEFAULT_VALUE) DEFAULT_VALUE_TEXT, CAST(ISNULL(VT.HAS_TAGS, 0) AS BIT) HAS_TAGS
             FROM
             (
                 SELECT VLH.OBJECT_ID, VLH.NAME, VLH.GLOBAL_CODE, NULL ALL_VALUES, NULL LOCAL_VALUES, 3 TYPE, COALESCE(VLH.PARENT_ID, VLH.ELEMENT_ID) PARENT_ID, NULL CHANNEL, CAST(vlh.STATUS AS BIT) IS_ACTIVE, ISNULL(vd.VALUE,
                                                                                                                                                                                                                            CASE
                                                                                                                                                                                                                                WHEN vlh.COUNTRY_ID IS NULL
                                                                                                                                                                                                                                THEN NULL
                                                                                                                                                                                                                                ELSE ISNULL(v.TEXT_VALUE, '(Undefined)')
                                                                                                                                                                                                                            END) LOCAL_VALUE, COALESCE(vd.LOCAL_CODE, v.GLOBAL_CODE) LOCAL_CODE, vlh.LEVEL, VLH.ELEMENT_ID, NULL IS_LOV, VLL.NAME LEVEL_NAME, CAST(vlh.READONLY AS BIT) READONLY, NULL AS DESCRIPTION, NULL AS ACTIVATED_BY, NULL AS ACTIVATION_DATE, NULL AS DEFINED_BY, NULL AS DEFINITION_DATE, NULL DEACTIVATED_BY, NULL DEACTIVATION_DATE, vlh.ROOT_ID [ROOT_ID], NULL DATA_TYPE, NULL ATTRIBUTES, NULL DEFAULT_VALUE, NULL IS_REQUIRED
                 FROM
                 (
                     SELECT vlh.object_id, MAX(vlh.name) name, MAX(VLH.GLOBAL_CODE) GLOBAL_CODE, MAX(LEVEL) level, COALESCE(VLH.PARENT_ID, VLH.ELEMENT_ID) PARENT_ID, MAX(ELEMENT_ID) ELEMENT_ID, MAX(CAST(VLH.READONLY AS TINYINT)) [READONLY], MAX(VLH.VALUE_ID) VALUE_ID, ROOT_ID, MAX(vlh.STATUS) STATUS, COUNTRY_ID, MAX(parent_value_id) PARENT_VALUE_ID
                     FROM VALUE_LIST_HIERARCHY vlh
                     WHERE COALESCE(VLH.PARENT_ID, VLH.ELEMENT_ID) IS NOT NULL
                     GROUP BY vlh.ROOT_ID, vlh.object_id, COALESCE(VLH.PARENT_ID, VLH.ELEMENT_ID), COUNTRY_ID
                 ) VLH
                 LEFT JOIN CMT_VALUE_DETAIL vd ON vd.VALUE_id = vlh.VALUE_ID
                                                  AND vd.COUNTRY_ID = @COUNTRY_ID
                 LEFT JOIN CMT_VALUE_LIST_LEVEL VLL ON VLL.Element_ID = VLH.ELEMENT_ID
                                                       AND VLL.LEVEL = VLH.level
                 LEFT JOIN CMT_VALUE V ON V.OBJECT_ID = VLH.PARENT_VALUE_ID
                 UNION ALL
                 SELECT S.OBJECT_ID, S.NAME, NULL, NULL, NULL, 1, NULL, C.NAME, S.IS_ACTIVE, NULL, NULL, NULL, NULL, NULL, 'Schema', 0 READONLY, s.DESCRIPTION, s.ACTIVATED_BY, s.ACTIVATION_DATE, s.DEFINED_BY, s.DEFINITION_DATE, DEACTIVATED_BY, DEACTIVATION_DATE, s.OBJECT_ID, NULL, NULL ATTRIBUTES, NULL, NULL
                 FROM CMT_METADATA_SCHEMA S
                      INNER JOIN CMT_CHANNEL C ON C.OBJECT_ID = S.CHANNEL_ID
                 UNION ALL
                 SELECT SE.ELEMENT_ID, e.NAME, NULL, tv.[ALL], tv.TRANSLATED TRANSLATED, 2, SE.SCHEMA_ID, NULL, NULL, NULL, NULL, 0, se.ELEMENT_ID, CAST(CASE
                                                                                                                                                             WHEN et.NAME = 'LOV'
                                                                                                                                                             THEN 1
                                                                                                                                                             ELSE 0
                                                                                                                                                         END AS BIT), 'Element', E.READONLY, NULL AS DESCRIPTION, NULL AS ACTIVATED_BY, NULL AS ACTIVATION_DATE, NULL AS DEFINED_BY, NULL AS DEFINITION_DATE, NULL DEACTIVATED_BY, NULL DEACTIVATION_DATE, se.SCHEMA_ID, ET.NAME, E.ATTRIBUTES, SE.DEFAULT_VALUE, SE.IS_REQUIRED
                 FROM CMT_METADATA_ELEMENT E
                      INNER JOIN CMT_METADATA_SCHEMA_ELEMENT SE ON SE.ELEMENT_ID = E.OBJECT_ID
                      INNER JOIN CMT_ELEMENT_TYPE ET ON ET.OBJECT_ID = E.TYPE_ID
                      LEFT JOIN TRANSLATED_VALUES TV ON E.OBJECT_ID = TV.ELEMENT_ID
                                                        AND se.SCHEMA_ID = tv.SCHEMA_ID
                 WHERE E.STATUS > 0
             ) D
             LEFT JOIN CMT_VALUE v ON d.IS_LOV = 1
                                      AND CAST(v.OBJECT_ID AS NVARCHAR(40)) = d.DEFAULT_VALUE
             OUTER APPLY
             (
                 SELECT TOP 1 CONVERT(BIT, 1) HAS_TAGS, VALUE_ID
                 FROM CMT_VALUE_TAG
                 WHERE VALUE_ID = d.OBJECT_ID
             ) VT
             WHERE d.ROOT_ID IS NOT NULL
    END

GO


ALTER VIEW [dbo].[CMT_VIEW_METADATA_ELEMENT_TREE] AS
WITH VALUE_LIST_HIERARCHY AS
(
    SELECT
        V.OBJECT_ID,
        V.TEXT_VALUE PARENT_NAME,
        V.TEXT_VALUE NAME,
        4 TYPE,
        CAST(NULL AS uniqueidentifier) PARENT_ID,
        V.VALUE_LIST_ID AS VALUE_LIST_ID,
        V.CHILD_LIST_ID CHILD_LIST_ID,
        E.OBJECT_ID [ROOT_ID],
        E.OBJECT_ID AS ELEMENT_ID,
        1 [LEVEL],
        E.READONLY,
        v.STATUS,
        v.GLOBAL_CODE,
        v.COUNTRY_ID,
        v.PARENT_ID PARENT_VALUE_ID
    FROM CMT_VALUE V
    left JOIN CMT_METADATA_ELEMENT E ON E.VALUE_LIST_ID = V.VALUE_LIST_ID
    where v.EXTERNAL_ID is null or v.COUNTRY_ID is not null 
    UNION ALL
    SELECT
        V.OBJECT_ID,
        VLH.NAME PARENT_NAME,
        V.TEXT_VALUE,
        4 TYPE,
        VLH.OBJECT_ID,
        VLH.VALUE_LIST_ID,
        V.CHILD_LIST_ID,
        VLH.ROOT_ID,
        VLH.ELEMENT_ID,
        VLH.LEVEL + 1,
        VLH.READONLY,
        v.STATUS,
        v.GLOBAL_CODE,
        V.COUNTRY_ID,
        v.PARENT_ID
    FROM VALUE_LIST_HIERARCHY VLH
    INNER JOIN CMT_VALUE V
        ON VLH.CHILD_LIST_ID = V.VALUE_LIST_ID    
    where v.EXTERNAL_ID is null or v.COUNTRY_ID is not null 
)
 
SELECT 
    distinct D.*,
    C.CODE COUNTRY_CODE,
    dbo.GENERATE_IDENTIFIER(cast(d.object_id as nvarchar(40)) + ISNULL(d.local_value,'') + ISNULL(C.CODE,'') + isNull(cast(d.ROOT_ID as nvarchar(40)),'')) [UNIQUE_ID] ,
    ISNULL(v.TEXT_VALUE,d.DEFAULT_VALUE) DEFAULT_VALUE_TEXT,
    pv.PARENT_ID as PARENT_VALUE_ID,
	CONVERT(BIT, ISNULL(VT.HAS_TAGS,0)) HAS_TAGS
FROM (
SELECT 
    VLH.OBJECT_ID,
    VLH.NAME,
    NULL ALL_VALUES,
    NULL LOCAL_VALUES,
    c.OBJECT_ID COUNTRY_ID, 
    3 TYPE,
    vlh.PARENT_ID,
    CAST(VLH.status AS BIT) IS_ACTIVE,
    ISNULL(vd.VALUE, CASE WHEN vlh.COUNTRY_ID is null then null else ISNULL(v.TEXT_VALUE,'(Undefined)') end) LOCAL_VALUE, 
    VLH.LEVEL,
    VLH.ROOT_ID,
    NULL IS_LOV,
    VLL.NAME LEVEL_NAME,
    NULL DATA_TYPE,
    VLH.READONLY,
    VLH.GLOBAL_CODE,
    COALESCE(vd.LOCAL_CODE,v.GLOBAL_CODE) LOCAL_CODE,
    NULL ATTRIBUTES,
    NULL DEFAULT_VALUE
FROM (
    select 
        vlh.object_id,
        max(vlh.name) name,
        max(LEVEL) level,
        COALESCE(VLH.PARENT_ID,VLH.ROOT_ID) PARENT_ID,
        MAX(ELEMENT_ID) ELEMENT_ID,
        cast(max(cast(VLH.READONLY as int)) as bit) [READONLY],
        max(vlh.STATUS) status,
        max(vlh.GLOBAL_CODE) GLOBAL_CODE,
        COUNTRY_ID,
        max(parent_value_id) PARENT_VALUE_ID,
         vlh.ROOT_ID
    from VALUE_LIST_HIERARCHY vlh
    where COALESCE(VLH.PARENT_ID,VLH.ELEMENT_ID) is not null
    group by vlh.ROOT_ID, vlh.object_id, VLH.PARENT_ID, COUNTRY_ID
    )VLH 
CROSS JOIN CMT_COUNTRY C
left join CMT_VALUE_DETAIL vd
    on vd.VALUE_id = vlh.OBJECT_ID
        and vd.COUNTRY_ID = c.OBJECT_ID 
left join CMT_VALUE_LIST_LEVEL VLL
    on VLL.Element_ID = vlh.ELEMENT_ID
        and VLL.LEVEL = VLH.level
LEFT JOIN CMT_VALUE V ON V.OBJECT_ID = VLH.PARENT_VALUE_ID
WHERE VLH.COUNTRY_ID IS NULL OR VLH.COUNTRY_ID=C.OBJECT_ID


UNION ALL
SELECT 
    E.OBJECT_ID,
    E.NAME,
    NULL,
    NULL,
    NULL,
    2,
    E.GROUP_ID,
    CAST(e.STATUS as bit),
    NULL,
    0,
    NULL,
    CAST(CASE when et.NAME ='LOV' then 1 else 0 end as BIT),
    'Element',
    et.NAME,
    e.READONLY,
    null as GLOBAL_CODE,
    null as LOCAL_CODE,
    e.ATTRIBUTES,
    e.DEFAULT_VALUE
FROM CMT_METADATA_ELEMENT E
INNER JOIN CMT_ELEMENT_TYPE ET ON ET.OBJECT_ID = E.TYPE_ID
WHERE E.STATUS > 0
) D
left JOIN CMT_COUNTRY C ON C.OBJECT_ID = D.COUNTRY_ID
LEFT JOIN CMT_VALUE v on d.IS_LOV = 1 and CAST(v.OBJECT_ID as NVARCHAR(40)) = d.DEFAULT_VALUE
left join CMT_VALUE pv on pv.OBJECT_ID = d.OBJECT_ID and d.TYPE = 3
outer apply (Select TOP 1 CONVERT(BIT,1) HAS_TAGS, VALUE_ID from CMT_VALUE_TAG where VALUE_ID = d.OBJECT_ID) VT
GO

Exec RegisterSchemaVersion '0.1.190729.00000.0'
GO
