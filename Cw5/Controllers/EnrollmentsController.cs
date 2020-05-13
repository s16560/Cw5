using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Cw5.DTOs.Requests;
using Cw5.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Cw5.Controllers
{
    [Route("api/enrollments")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {
        private IStudentDbService _service;

        public EnrollmentsController(IStudentDbService service)
        {
            _service = service;
        }

        [HttpPost]
        public IActionResult EnrollStudent(EnrollStudentRequest request)
        {
            var response = _service.EnrollStudent(request);

            if (response.ResultCode.Equals("400")) {
                return BadRequest(response.Message);
            }

            return Created(response.Message, response.Obj);
        }

        [HttpPost("{promotions}")]       
        public IActionResult PromoteStudents(PromoteStudentsRequest request)
        {
            var response = _service.PromoteStudents(request);

            if (response.ResultCode.Equals("400"))
            {
                return BadRequest(response.Message);
            }

            if (response.ResultCode.Equals("404"))
            {
                return NotFound(response.Message);
            }

            return Created(response.Message, response.Obj);

        }


    }
}


