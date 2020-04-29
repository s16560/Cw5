using System.Collections.Generic;

namespace Cw5.Services
{
    public interface IStudentsDal
    {
        public IEnumerable<Student> GetStudents();
        public IEnumerable<Student> GetStudent(string indexNumber);
    }

}

