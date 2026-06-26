using Xunit;
using task02;

public class StudentServiceTests
{
    private List<Student> _testStudents;
    private StudentService _service;

    public StudentServiceTests()
    {
        _testStudents = new List<Student>
        {
            new() { Name = "Иван", Faculty = "ФИТ", Grades = new List<int> { 5, 4, 5 } },
            new() { Name = "Анна", Faculty = "ФИТ", Grades = new List<int> { 3, 4, 3 } },
            new() { Name = "Петр", Faculty = "Экономика", Grades = new List<int> { 5, 5, 5 } }
        };
        _service = new StudentService(_testStudents);
    }

    [Fact]
    public void GetStudentsByFaculty_ReturnsCorrectStudents()
    {
        var result = _service.GetStudentsByFaculty("ФИТ").ToList();
        Assert.Equal(2, result.Count);
        Assert.True(result.All(s => s.Faculty == "ФИТ"));
    }

    [Fact]
    public void GetStudentsWithMinAverageGrade_ReturnsCorrectStudents()
    {
        var result = _service.GetStudentsWithMinAverageGrade(4.0).ToList();
        Assert.Equal(2, result.Count);
        Assert.True(result.All(s => s.Grades.Average() > 4.0));
    }

    [Fact]
    public void GetStudentsOrderedByName_ReturnsCorrectStudents()
    {
        var result = _service.GetStudentsOrderedByName().ToList();
        Assert.Equal(3, result.Count);
        Assert.True(result[0].Name == "Анна");
        Assert.True(result[^1].Name == "Петр");
    }

    [Fact]
    public void GroupStudentsByFaculty_ReturnsCorrectStudents()
    {
        var result = _service.GroupStudentsByFaculty();
        Assert.Equal(2, result["ФИТ"].Count());
        Assert.Single(result["Экономика"]);
        Assert.Equal("Петр", result["Экономика"][0].Name);
        Assert.Equal("Иван", result["ФИТ"][0].Name);
        Assert.Equal("Анна", result["ФИТ"][1].Name);
    }

    [Fact]
    public void GetFacultyWithHighestAverageGrade_ReturnsCorrectFaculty()
    {
        var result = _service.GetFacultyWithHighestAverageGrade();
        Assert.Equal("Экономика", result);
    }
}