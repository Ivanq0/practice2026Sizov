using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace task13
{
    public class StudentManager
    {
        private JsonSerializerOptions GetSettings()
        {
            var options = new JsonSerializerOptions();

            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.WriteIndented = true;
            options.Converters.Add(new DateConverter());

            return options;
        }

        public string Serialize(Student student)
        {
            return JsonSerializer.Serialize(student, GetSettings());
        }

        public Student Deserialize(string json)
        {
            var student = JsonSerializer.Deserialize<Student>(json, GetSettings());

            if (student.FirstName == "" || student.FirstName == null)
            {
                throw new Exception("Ошибка: У студента нет имени");
            }

            if (student.BirthDate.Year < 1900)
            {
                throw new Exception("Ошибка: Год рождения не может быть меньше 1900");
            }

            return student;
        }

        public void SaveToFile(string path, Student student)
        {
            string json = Serialize(student);
            File.WriteAllText(path, json);
        }

        public Student LoadFromFile(string path)
        {
            string json = File.ReadAllText(path);
            return Deserialize(json);
        }
    }
}