namespace CMOS.Data_Models
{
    public class Position
    {
        public bool IsUpdate { get; set; }
        public bool IsWeight { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public double Weight { get; set; }
        public string ShortName { get; set; }
        public int Norm { get; set; }
        public int Rate { get; set; }
        public string Loading { get; set; }
        public string Order { get; set; }
        public string Color { get; set; }
        public int Id { get; set; }
    }
}