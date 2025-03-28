using System.ComponentModel.DataAnnotations;

namespace EamProject3.Models
{
    public class RequestSituationModel
    {
        public required Request Request { get; set; }
        public List<Situation> Situations { get; set; }
    }
}
