using CMT.BO;
using CMT.DL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMT.UT.DataMockups
{
    public class ElementDataMockup
    {
        public List<ElementType> ElementTypes { get; private set; }
        public List<MetadataElement> MetadataElements { get; private set; }
        public List<ValueDetail> ValueDetails { get; private set; }
        public List<Value> Values { get; private set; }
        public List<ValueList> ValueLists { get; private set; }
        public List<ValueListLevel> ValueListLevels { get; private set; }

        Guid Value1Id = Guid.NewGuid();
        Guid Value2Id = Guid.NewGuid();
        Guid Value3Id = Guid.NewGuid();
        Guid ValueListLevel1Id = Guid.NewGuid();
        Guid ValueListLevel2Id = Guid.NewGuid();
        Guid ValueListLevel3Id = Guid.NewGuid();

        public ElementDataMockup()
        {
            CreateValueDetails();
            CreateValueLists();
            CreateElementTypes();
            CreateValues();
        }

        void CreateValueDetails()
        {

            ValueDetails = (new ValueDetail[]
            {
                new ValueDetail(){ ObjectId = Guid.NewGuid(), CountryId = Guid.NewGuid(), ValueId = Value1Id, Type = 0 },
                new ValueDetail(){ ObjectId = Guid.NewGuid(), CountryId = Guid.NewGuid(), ValueId = Value1Id, Type = 1 },
                new ValueDetail(){ ObjectId = Guid.NewGuid(), CountryId = Guid.NewGuid(), ValueId = Value1Id, Type = 2 },
                new ValueDetail(){ ObjectId = Guid.NewGuid(), CountryId = Guid.NewGuid(), ValueId = Value2Id, Type = 0 },
                new ValueDetail(){ ObjectId = Guid.NewGuid(), CountryId = Guid.NewGuid(), ValueId = Value2Id, Type = 1 },
                new ValueDetail(){ ObjectId = Guid.NewGuid(), CountryId = Guid.NewGuid(), ValueId = Value2Id, Type = 2 },
                new ValueDetail(){ ObjectId = Guid.NewGuid(), CountryId = Guid.NewGuid(), ValueId = Value3Id, Type = 0 },
                new ValueDetail(){ ObjectId = Guid.NewGuid(), CountryId = Guid.NewGuid(), ValueId = Value3Id, Type = 1 },
                new ValueDetail(){ ObjectId = Guid.NewGuid(), CountryId = Guid.NewGuid(), ValueId = Value3Id, Type = 2 },
            }).ToList();
        }

        void CreateValueLists()
        {
            ValueLists = (new ValueList[]
            {
                new ValueList() { Name = "Level1", ObjectId = ValueListLevel1Id},
                new ValueList() { Name = "Level2", ObjectId = ValueListLevel2Id},
                new ValueList() { Name = "Level3", ObjectId = ValueListLevel3Id}
            }).ToList();
        }

        public void CreateValueListLevels(Guid elementId)
        {
            ValueListLevels = (new ValueListLevel[]
            {
                new ValueListLevel() { Name = "Level1", ObjectId = Guid.NewGuid(), Level = 1, ElementId = elementId},
                new ValueListLevel() { Name = "Level2", ObjectId = Guid.NewGuid(), Level = 2, ElementId = elementId},
                new ValueListLevel() { Name = "Level3", ObjectId = Guid.NewGuid(), Level = 3, ElementId = elementId}
            }).ToList();
        }


        void CreateValues()
        {
            Values = (new Value[]
            {
                new Value() {
                    ObjectId = Value1Id, ValueListId = ValueListLevel1Id, ChildListId = ValueListLevel2Id, ValueList = ValueLists.FirstOrDefault(p=>p.ObjectId == ValueListLevel1Id),
                    ValueDetails = ValueDetails.Where(p=>p.ValueId == Value1Id).ToList()
                },
                new Value() {
                    ObjectId = Value2Id, ValueListId = ValueListLevel2Id, ChildListId = ValueListLevel3Id, ValueList = ValueLists.FirstOrDefault(p=>p.ObjectId == ValueListLevel2Id),
                    ValueDetails = ValueDetails.Where(p=>p.ValueId == Value2Id).ToList()
                },
                new Value() {
                    ObjectId = Value3Id, ValueListId = ValueListLevel3Id, ValueList = ValueLists.FirstOrDefault(p=>p.ObjectId == ValueListLevel3Id),
                    ValueDetails = ValueDetails.Where(p=>p.ValueId == Value3Id).ToList()
                }
            }).ToList();
        }


        void CreateElementTypes()
        {
            ElementTypes = (new ElementType[] {
                new ElementType() { CreateDate = DateTime.Now, CreateUser = "system", Name = "LOV", ObjectId = Guid.NewGuid(), Type = ElementTypeType.Lov },
                new ElementType() { CreateDate = DateTime.Now, CreateUser = "system", Name = "Text", ObjectId = Guid.NewGuid(), Type = ElementTypeType.Unset }
            }).ToList();
        }
        public void CreateMetadataElements(Guid lovMetadataElementId)
        {
            MetadataElements = (
                new MetadataElement[] {
                    new MetadataElement() {
                        CreateDate = DateTime.Now,
                        CreateUser = "system",
                        Name = "Test Element",
                        ObjectId = lovMetadataElementId,
                        TypeId = ElementTypes.Single(_=>_.Type == ElementTypeType.Lov).ObjectId,
                        ElementType = ElementTypes.Single(_=>_.Type == ElementTypeType.Lov),
                        ValueListId = ValueLists.First().ObjectId,
                        ValueListLevels = ValueListLevels.Where(_=>_.ElementId == lovMetadataElementId).ToList()
                    }
                }).ToList();


        }
    }
}
