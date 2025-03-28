namespace EamProject3.Models
{
    public class CreateNewRequestModel
    {
        public Course Course { get; set; }
        public int ModuleId { get; set; }
        public int TeacherId { get; set; }
        public DateTime ExamDateTime { get; set; }
        public int DurationMin { get; set; }

        public List<Module> Modules { get; set; }
        public List<User> Teachers { get; set; }

    }
}
