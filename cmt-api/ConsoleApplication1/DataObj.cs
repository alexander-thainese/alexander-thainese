namespace CMT.SchemaImporter
{

    public class CrtElement
    {
        public string id { get; set; }
        public string name { get; set; }
        public int active { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public Pivot pivot { get; set; }
        public Value[] values { get; set; }
        public object predefined_value { get; set; }
        public int required { get; set; }
        public string validation { get; set; }
        public string field_type { get; set; }
        public int mdt_property_set_id { get; set; }
        public string meta_master_id { get; set; }
        public string value { get; set; }
    }

    public class Pivot
    {
        public string meta_valuable_id { get; set; }
        public string meta_value_id { get; set; }
    }

    public class Value
    {
        public string id { get; set; }
        public string value { get; set; }
        public int active { get; set; }
        public int editable { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public object attributes { get; set; }
        public Pivot1 pivot { get; set; }
    }

    public class Pivot1
    {
        public string meta_valuable_id { get; set; }
        public string meta_value_id { get; set; }
        public string meta_data_type_id { get; set; }
        public int id { get; set; }
    }


    //public class Schema
    //{
    //    public CrtElement[] Property1 { get; set; }
    //}

    //public class CrtElement
    //{
    //    public string id { get; set; }
    //    public string name { get; set; }
    //    public int active { get; set; }
    //    public string created_at { get; set; }
    //    public string updated_at { get; set; }
    //    public Pivot pivot { get; set; }
    //    public CrtValue[] values { get; set; }
    //    public object predefined_value { get; set; }
    //    public int required { get; set; }
    //    public string validation { get; set; }
    //    public string field_type { get; set; }
    //    public string value { get; set; }
    //}

    //public class Pivot
    //{
    //    public string meta_value_id { get; set; }
    //}

    //public class CrtValue
    //{
    //    public string id { get; set; }
    //    public string value { get; set; }
    //    public int active { get; set; }
    //    public int editable { get; set; }
    //    public string created_at { get; set; }
    //    public string updated_at { get; set; }
    //    public object[] attributes { get; set; }
    //    public Pivot1 pivot { get; set; }
    //}

    //public class Pivot1
    //{
    //    public string meta_value_id { get; set; }
    //    public string meta_data_type_id { get; set; }
    //}

}
