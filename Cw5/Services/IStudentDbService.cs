using Cw5.DTOs.Requests;
using Cw5.DTOs.Responses;

namespace Cw5.Services
{
    public interface IStudentDbService
    {
        Response EnrollStudent(EnrollStudentRequest request);
        Response PromoteStudents(PromoteStudentsRequest request);
        bool StudentExists(string IndexNumber);
    }
}
