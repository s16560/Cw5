
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Cw5.Services
{
    public class SqlServerDbDal : IStudentsDal
    {
        private const string ConString = "Data Source=db-mssql;Initial Catalog=s16560;Integrated Security=True";


        public IEnumerable<Student> GetStudents()
        {
            var list = new List<Student>();

            using (SqlConnection con = new SqlConnection(ConString))
            using (SqlCommand com = new SqlCommand())
            {
                com.Connection = con;

                com.CommandText = @"select s.FirstName, s.LastName, s.BirthDate, st.Name, e.Semester
                                        from student s
                                        join enrollment e
                                        on s.IdEnrollment = e.IdEnrollment
                                        join Studies st
                                        on e.IdStudy = st.IdStudy";

                con.Open();
                SqlDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    var st = new Student();

                    st.FirstName = dr["FirstName"].ToString();
                    st.LastName = dr["LastName"].ToString();
                    st.BirthDate = dr["BirthDate"].ToString();
                    st.Studies = dr["Name"].ToString();
                    st.Semester = dr["Semester"].ToString();

                    list.Add(st);
                }

            }


            return list;
        }


        public IEnumerable<Student> GetStudent(string indexNumber)
        {
            var list = new List<Student>();

            using (SqlConnection con = new SqlConnection(ConString))
            using (SqlCommand com = new SqlCommand())
            {
                com.Connection = con;

                com.CommandText = @"select s.FirstName, s.LastName, s.BirthDate, st.Name, e.Semester
                                        from student s
                                        join enrollment e
                                        on s.IdEnrollment = e.IdEnrollment
                                        join Studies st
                                        on e.IdStudy = st.IdStudy
                                        where indexNumber=@index";

                com.Parameters.AddWithValue("index", indexNumber);

                con.Open();
                var dr = com.ExecuteReader();

                while (dr.Read())
                {
                    var st = new Student();

                    st.FirstName = dr["FirstName"].ToString();
                    st.LastName = dr["LastName"].ToString();
                    st.BirthDate = dr["BirthDate"].ToString();
                    st.Studies = dr["Name"].ToString();
                    st.Semester = dr["Semester"].ToString();

                    list.Add(st);
                }
            }

            return list;
        }


    }
}

