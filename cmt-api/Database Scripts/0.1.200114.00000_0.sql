

ALTER PROCEDURE [dbo].[GET_VALUES_ATTRIBUTES]
AS
select v.TEXT_VALUE, v.EXTERNAL_ID, v.OBJECT_ID as VALUE_ID, v.COUNTRY_ID, vd.ATTRIBUTE_ELEMENT_ID, vd.ATTRIBUTE_VALUE_ID 
, me.NAME as ELEMENT_NAME, av.TEXT_VALUE as ATTRIBUTE_VALUE, me.ATTRIBUTES as ELEMENT_TYPE
from CMT_VALUE v
left join CMT_VALUE_DETAIL vd on vd.VALUE_ID = v.OBJECT_ID 
left join CMT_METADATA_ELEMENT me on me.OBJECT_ID =  vd.ATTRIBUTE_ELEMENT_ID
left join CMT_VALUE av on av.OBJECT_ID = vd.ATTRIBUTE_VALUE_ID
 where v.VALUE_LIST_ID is null
 and v.object_id in (
select parent_id from cmt_value where parent_id is not null
)
and ATTRIBUTE_ELEMENT_ID is not null and ATTRIBUTE_VALUE_ID is not null 
order by v.TEXT_VALUE


GO

exec RegisterSchemaVersion '0.1.200114.00000.0'
GO



