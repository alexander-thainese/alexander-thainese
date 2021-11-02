namespace CMT.BO.Admin
{
    public class ListValue
    {
        public ListValue(string name, string value)
        {
            Name = name;
            Value = value;
        }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
