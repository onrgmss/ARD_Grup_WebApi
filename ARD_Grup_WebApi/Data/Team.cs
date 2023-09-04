using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARD_Grup_WebApi.Data
{
    public class Team
    {
        [Key]
        public int TeamId { get; set; }
        public string TeamName { get; set; } 

        public virtual ICollection<User> TeamMembers{ get; set; }


    }
}
