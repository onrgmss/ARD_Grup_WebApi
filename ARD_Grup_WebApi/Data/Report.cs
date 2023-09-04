using System.ComponentModel.DataAnnotations;

namespace ARD_Grup_WebApi.Data
{

    public class Report 
        {
            [Key]
            public int Id { get; set; }

            public string ReportName { get; set; }

            public DateTime CreatedAt { get; set; } = DateTime.Now;

            public DateTime LastUpdatedAt { get; set; }

             public int CurrentUserId { get; set; }
             public virtual User CurrentUser { get; set; }
            public string ActivitiesCarriedOut { get; set; }

            public string ProblemsRisks { get; set; }

            public string WaitingActivities { get; set; }

            public string RequestCommentsAndSuggestions { get; set; }
        }
    }

