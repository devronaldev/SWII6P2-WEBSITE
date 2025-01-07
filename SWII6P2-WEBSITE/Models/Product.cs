namespace SWII6P2_WEBSITE.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public float Price { get; set; }
        public bool Status { get; set; }
        public int RecordId { get; set; }
        public int LastUpdaterId { get; set; }
    }
}
