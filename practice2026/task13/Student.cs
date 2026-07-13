using System;
using System.Collections.Generic;

namespace task13
{
    public class Subject
    {
        public string Name { get; set; }
        public int Grade { get; set; }
    }

    public class Student
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }

        public List<Subject> Grades { get; set; } = new List<Subject>();
    }
}