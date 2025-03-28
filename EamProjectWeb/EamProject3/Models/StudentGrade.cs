namespace EamProject3.Models
{
    public class StudentGrade
    {
        public int StudentId { get; set; }
        public User? Student { get; set; }
        public decimal Grade { get; set; }
    }
}
