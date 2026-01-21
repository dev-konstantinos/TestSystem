namespace TestSystem.Entities.DTOs.Teacher
{
    public class TeacherDashboardDto
    {
        public int TeacherId { get; set; }
        public int StudentsCount { get; set; }
        public int TestsCount { get; set; }
        public int ResultsCount { get; set; }
    }
}
