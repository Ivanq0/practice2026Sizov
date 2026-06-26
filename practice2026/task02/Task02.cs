namespace task02
{
    public class Student
    {
        public string Name { get; set; }
        public string Faculty { get; set; }
        public List<int> Grades { get; set; }
    }

    public class StudentService
    {
        private readonly List<Student> _students;

        public StudentService(List<Student> students) => _students = students;

        public IEnumerable<Student> GetStudentsByFaculty(string faculty)
        {
            var query = from student in _students
                        where student.Faculty == faculty
                        select student;

            return query;
        }

        public IEnumerable<Student> GetStudentsWithMinAverageGrade(double minAverageGrade)
        {
            var query = from student in _students
                        where student.Grades.Average() > minAverageGrade
                        select student;

            return query;
        }

        public IEnumerable<Student> GetStudentsOrderedByName()
        {
            var query = from student in _students
                        orderby student.Name
                        select student;

            return query;
        }

        public Dictionary<string, List<Student>> GroupStudentsByFaculty() // Реализовал через Dictionary
        {
            var query = from student in _students
                        group student by student.Faculty;

            return query.ToDictionary(k => k.Key, v => v.ToList());
        }

        public string GetFacultyWithHighestAverageGrade()
        {
            var query = from student in _students
                        group student by student.Faculty into facultyGroup
                        orderby facultyGroup.Average(s => s.Grades.Average()) descending
                        select facultyGroup.Key;

            return query.FirstOrDefault();
        }
    }
}
