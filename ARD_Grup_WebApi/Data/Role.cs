namespace ARD_Grup_WebApi.Data
{
    public class Role
    {
        public int Id { get; set; } 
        public virtual ICollection<User> Users { get; set; }
        public string Name { get; set; }
    }
}
