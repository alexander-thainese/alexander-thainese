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
    pv.PARENT_ID as PARENT_VALUE_ID
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
GO

exec RegisterSchemaVersion '0.1.190607.00000.0'
go
