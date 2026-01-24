namespace TestSystem.Entities.DTOs.Student
{
    public class StudentTestSubmitDto
    {
        public int TestId { get; set; }
        public Dictionary<int, int> Answers { get; set; } = new();
    }
}
