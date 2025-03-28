namespace EamProject3.Models
{
    public class ModuleGradesModel
    {
        public User Teacher { get; set; }
        public ModuleSelectionViewModel SelectedModule { get; set; }
        public List<StudentGrade> StudentGrades { get; set; }
        public List<User> Students { get; set; }
    }
}
