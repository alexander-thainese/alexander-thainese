namespace CMT.BL.DataDistinctor
{
    public class BrandData
    {
        public string PfizerBrandId { get; set; }
        public string PfizerBrandName { get; set; }
        public string GlobalPfizerBrandId { get; set; }
        public string GlobalPfizerBrandName { get; set; }

        public static BrandData Create(object[] row)
        {
            if (row[0] == null || row[0].GetType() != typeof(string) || string.IsNullOrEmpty((string)row[0]))
            {
                return null;
            }

            if (row[1] == null || row[1].GetType() != typeof(string) || string.IsNullOrEmpty((string)row[1]))
            {
                return null;
            }

            if (row[3] == null || row[3].GetType() != typeof(string) || string.IsNullOrEmpty((string)row[3]))
            {
                return null;
            }

            if (row[4] == null || row[4].GetType() != typeof(string) || string.IsNullOrEmpty((string)row[4]))
            {
                return null;
            }

            return new BrandData()
            {
                PfizerBrandId = (string)row[0],
                PfizerBrandName = (string)row[1],
                GlobalPfizerBrandId = (string)row[3],
                GlobalPfizerBrandName = (string)row[4]
            };
        }
    }
}