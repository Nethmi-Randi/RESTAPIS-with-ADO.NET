using ADOCRUD.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace ADOCRUD.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public CourseController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET ALL COURSES
        [HttpGet]
        [Route("GetAllCourses")]
        public async Task<IActionResult> GetAllCourses()
        {
            List<CourseModel> courseModels = new List<CourseModel>();
            DataTable dt = new DataTable();
            SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DbConnection"));
            SqlCommand cmd = new SqlCommand("SELECT * FROM Courses", con);
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            adapter.Fill(dt);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                CourseModel courseModel = new CourseModel();
                courseModel.courseId = Convert.ToInt32(dt.Rows[i]["courseId"]);
                courseModel.courseTitle = dt.Rows[i]["courseTitle"].ToString();
                courseModel.maxStudents = Convert.ToInt32(dt.Rows[i]["maxStudents"]);
                courseModels.Add(courseModel);
            }

            return Ok(courseModels);
        }

        // GET A COURSE BY ID
        [HttpGet]
        [Route("GetCourseById/{id}")]
        public async Task<IActionResult> GetCourseById(int id)
        {
            CourseModel courseModel = new CourseModel();
            DataTable dt = new DataTable();
            SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DbConnection"));
            SqlCommand cmd = new SqlCommand("SELECT * FROM Courses WHERE courseId = @id", con);
            cmd.Parameters.AddWithValue("@id", id);
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            adapter.Fill(dt);
            if (dt.Rows.Count == 1)
            {
                courseModel.courseId = Convert.ToInt32(dt.Rows[0]["courseId"]);
                courseModel.courseTitle = dt.Rows[0]["courseTitle"].ToString();
                courseModel.maxStudents = Convert.ToInt32(dt.Rows[0]["maxStudents"]);

                return Ok(courseModel);
            }

            return NotFound("Course not found.");
        }

        //GET STUDENTS BY COURSE ID 
        [HttpGet]
        [Route("GetStudentsByCourseId/{courseId}")]
        public async Task<IActionResult> GetStudentsByCourseId(int courseId)
        {
            SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DbConnection"));
            SqlCommand checkCourseCmd = new SqlCommand("SELECT COUNT(1) FROM Courses WHERE courseId = @courseId", con);
            checkCourseCmd.Parameters.AddWithValue("@courseId", courseId);
            con.Open();
            int courseExists = Convert.ToInt32(checkCourseCmd.ExecuteScalar());
            con.Close();

            if (courseExists == 0)
            {
                return BadRequest("Invalid courseId");
            }

            List<StudentModel> studentModels = new List<StudentModel>();
            DataTable dt = new DataTable();
            SqlCommand cmd = new SqlCommand("SELECT * FROM Students WHERE courseId = @courseId", con);
            cmd.Parameters.AddWithValue("@courseId", courseId);
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            adapter.Fill(dt);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                StudentModel studentModel = new StudentModel();
                studentModel.studentId = Convert.ToInt32(dt.Rows[i]["studentId"]);
                studentModel.studentName = dt.Rows[i]["studentName"].ToString();
                
                studentModels.Add(studentModel);
            }

            if (studentModels.Count == 0)
            {
                return NotFound("No students found for this course");
            }

            return Ok(studentModels);
        }

        //POST A COURSE
        [HttpPost]
        [Route("AddCourse")]
        public async Task<IActionResult> AddCourse(CourseModel course)
        {
            if (course == null || string.IsNullOrEmpty(course.courseTitle) || course.courseId <= 0 || course.maxStudents <0)
            {
                return BadRequest("Invalid course data");
            }

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DbConnection")))
            {
                SqlCommand checkCourseCmd = new SqlCommand("SELECT COUNT(1) FROM Courses WHERE courseId = @courseId", con);
                checkCourseCmd.Parameters.AddWithValue("@courseId", course.courseId);
                con.Open();
                int courseExists = Convert.ToInt32(checkCourseCmd.ExecuteScalar());
                con.Close();

                if (courseExists > 0)
                {
                    return BadRequest("Course Id already exists");
                }

                SqlCommand cmd = new SqlCommand(
                    "INSERT INTO Courses (courseId, courseTitle,maxStudents) VALUES (@courseId, @courseTitle, @maxStudents)", con);

                cmd.Parameters.AddWithValue("@courseId", course.courseId);
                cmd.Parameters.AddWithValue("@courseTitle", course.courseTitle);
                cmd.Parameters.AddWithValue("@maxStudents", course.maxStudents);

                con.Open();
                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                con.Close();

                if (rowsAffected > 0)
                {
                    return Ok("Course added successfully");
                }
                else
                {
                    return StatusCode(500, "Failed to add course");
                }
            }
        }

        //PUT COURSE 
        [HttpPut]
        [Route("UpdateCourse")]
        public async Task<IActionResult> UpdateCourse(CourseModel course)
        {
            if (course == null || string.IsNullOrEmpty(course.courseTitle) || course.courseId <= 0 || course.maxStudents<0)
            {
                return BadRequest("Invalid course data");
            }

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DbConnection")))
            {
                SqlCommand checkCourseCmd = new SqlCommand("SELECT COUNT(1) FROM Courses WHERE courseId = @courseId", con);
                checkCourseCmd.Parameters.AddWithValue("@courseId", course.courseId);
                con.Open();
                int courseExists = Convert.ToInt32(checkCourseCmd.ExecuteScalar());
                con.Close();

                if (courseExists == 0)
                {
                    return NotFound("Invalid course id");
                }

                SqlCommand cmd = new SqlCommand(
                    "UPDATE Courses SET courseTitle = @courseTitle , maxStudents = @maxStudents WHERE courseId = @courseId", con);
               
                cmd.Parameters.AddWithValue("@courseTitle", course.courseTitle);
                cmd.Parameters.AddWithValue("@maxStudents", course.maxStudents);
                cmd.Parameters.AddWithValue("@courseId", course.courseId);

                con.Open();
                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                con.Close();

                if (rowsAffected > 0)
                {
                    return Ok("Course updated successfully");
                }
                else
                {
                    return StatusCode(500, "Failed to update course");
                }
            }
        }

        //DELETE A COURSE 
        [HttpDelete]
        [Route("DeleteCourse/{courseId}")]
        public async Task<IActionResult> DeleteCourse(int courseId)
        {
            if (courseId <= 0)
            {
                return BadRequest("Invalid course ID");
            }

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DbConnection")))
            {
                SqlCommand checkCourseCmd = new SqlCommand("SELECT COUNT(1) FROM Courses WHERE courseId = @courseId", con);
                checkCourseCmd.Parameters.AddWithValue("@courseId", courseId);
                con.Open();
                int courseExists = Convert.ToInt32(checkCourseCmd.ExecuteScalar());

                if (courseExists == 0)
                {
                    con.Close();
                    return NotFound("Course not found");
                }

                SqlCommand checkStudentsCmd = new SqlCommand("SELECT COUNT(1) FROM Students WHERE courseId = @courseId", con);
                checkStudentsCmd.Parameters.AddWithValue("@courseId", courseId);
                int studentsEnrolled = Convert.ToInt32(checkStudentsCmd.ExecuteScalar());

                if (studentsEnrolled > 0)
                {
                    con.Close();
                    return BadRequest("Can't remove, students have enrolled for this course");
                }

                SqlCommand deleteCmd = new SqlCommand("DELETE FROM Courses WHERE courseId = @courseId", con);
                deleteCmd.Parameters.AddWithValue("@courseId", courseId);

                int rowsAffected = await deleteCmd.ExecuteNonQueryAsync();
                con.Close();

                if (rowsAffected > 0)
                {
                    return Ok("Course deleted successfully");
                }
                else
                {
                    return StatusCode(500, "Failed to delete course");
                }
            }
        }




    }
}

