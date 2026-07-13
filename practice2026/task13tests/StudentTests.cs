using System;
using System.IO;
using Xunit;
using task13;

public class StudentTests
{
    [Fact]
    public void Test_IgnoreNullAndDateFormat()
    {
        var manager = new StudentManager();
        var student = new Student
        {
            FirstName = "Антон",
            LastName = null,
            BirthDate = new DateTime(2005, 1, 15)
        };

        string json = manager.Serialize(student);

        Assert.Contains("2005-01-15", json);
        Assert.DoesNotContain("LastName", json);
    }

    [Fact]
    public void Test_DeserializeAndValidate()
    {
        var manager = new StudentManager();
        string json = "{\"FirstName\":\"Максим\",\"BirthDate\":\"2003-10-12\"}";

        var student = manager.Deserialize(json);

        Assert.Equal("Максим", student.FirstName);
        Assert.Equal(2003, student.BirthDate.Year);
    }

    [Fact]
    public void Test_EmptyNameThrowsException()
    {
        var manager = new StudentManager();
        string badJson = "{\"FirstName\":\"\",\"BirthDate\":\"2003-10-12\"}";

        Assert.Throws<Exception>(() => manager.Deserialize(badJson));
    }

    [Fact]
    public void Test_SaveAndLoadFile()
    {
        var manager = new StudentManager();
        var student = new Student
        {
            FirstName = "Илья",
            BirthDate = new DateTime(2000, 5, 5)
        };
        string filePath = "test_student.json";
        manager.SaveToFile(filePath, student);
        var loadedStudent = manager.LoadFromFile(filePath);

        Assert.Equal("Илья", loadedStudent.FirstName);
        File.Delete(filePath);
    }
}