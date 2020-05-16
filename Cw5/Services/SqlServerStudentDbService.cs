using Cw5.DTOs.Requests;
using Cw5.DTOs.Responses;
using System.Data;
using System.Data.SqlClient;

namespace Cw5.Services
{
    public class SqlServerStudentDbService : IStudentDbService
    {
        
            private const string ConString = "Data Source=db-mssql;Initial Catalog=s16560;Integrated Security=True";
          
            public Response EnrollStudent(EnrollStudentRequest request)
            {
                using (var con = new SqlConnection(ConString))
                using (var com = new SqlCommand())
                {
                    com.Connection = con;
                    con.Open();
                    var tran = con.BeginTransaction();
                    com.Transaction = tran;

                    try
                    {
                        //czy studia istniaja
                        com.CommandText = @"select IdStudy 
                                            from studies 
                                            where name = @name";
                        com.Parameters.AddWithValue("name", request.Studies);

                        var dr = com.ExecuteReader();

                        if (!dr.Read())
                        {
                            dr.Close();
                            tran.Rollback();
                            return new Response("400", "Studia " + request.Studies + " nie istnieją", null);
                        }

                        int idStudy = (int)dr["IdStudy"];
                        dr.Close();

                        //czy istnieje wpis dla studiow, z semestrem 1
                        com.CommandText = @"select idEnrollment 
                                            from enrollment 
                                            where semester = 1 and idStudy = @idStudy";
                        com.Parameters.AddWithValue("idStudy", idStudy);

                        int idEnrollment;

                        com.Transaction = tran;
                        dr = com.ExecuteReader();
                        if (dr.Read())
                        {
                            idEnrollment = (int)dr["idEnrollment"];
                            dr.Close();
                        }
                        else
                        {
                            dr.Close();
                            com.CommandText = @"insert into Enrollment
                                                            values((SELECT MAX(idEnrollment) + 1 FROM Enrollment),
                                                            1,
                                                            @idStudy,
                                                            getDate())";
                            com.Transaction = tran;
                            com.ExecuteNonQuery();
                            dr.Close();

                            com.CommandText = @"select idEnrollment 
                                                        from enrollment 
                                                        where semester = 1 and idStudy = @idStudy";
                            com.Transaction = tran;
                            dr = com.ExecuteReader();
                            dr.Read();
                            idEnrollment = (int)dr["idEnrollment"];
                            dr.Close();
                        }

                        //dodanie studenta
                        com.CommandText = @"insert into student
                                            values(@IndexNumber, @FirstName, @LastName, @BirthDate, 
                                            (select MAX(idEnrollment) from Enrollment))";
                        com.Parameters.AddWithValue("IndexNumber", request.IndexNumber);
                        com.Parameters.AddWithValue("FirstName", request.FirstName);
                        com.Parameters.AddWithValue("LastName", request.LastName);
                        com.Parameters.AddWithValue("BirthDate", request.BirthDate);

                        com.ExecuteNonQuery();

                        tran.Commit();

                        var enrollment = new EnrollStudentResponse();
                        enrollment.IndexNumber = request.IndexNumber;
                        enrollment.FirstName = request.FirstName;
                        enrollment.LastName = request.LastName;
                        enrollment.Studies = request.Studies;
                        enrollment.Semester = 1;

                        return new Response("201", "Zapisano studenta", enrollment);

                    }
                    catch (SqlException exc)
                    {
                        tran.Rollback();
                    }
                }
                return new Response("400", "Bad request", null);

        }

                
            public Response PromoteStudents(PromoteStudentsRequest request)
            {

                using (var con = new SqlConnection(ConString))
                using (var com = new SqlCommand())
                {
                    com.Connection = con;
                    con.Open();
                    var tran = con.BeginTransaction();
                    com.Transaction = tran;

                    try
                    {
                        com.CommandText = @"select *
                                            from enrollment join studies
                                            on enrollment.idStudy = studies.idStudy
                                            where name = @name and semester = @semester";
                        com.Parameters.AddWithValue("name", request.Studies);
                        com.Parameters.AddWithValue("semester", request.Semester);

                        var dr = com.ExecuteReader();

                        if (!dr.Read())
                        {
                            dr.Close();
                            tran.Rollback();
                            return new Response("404", "Wpis dla " + 
                                                request.Studies + " semestr " + request.Semester +
                                                " nie istnieje", null);
                        }
                        dr.Close();

                        com.Parameters.Clear();
                        com.CommandType = CommandType.StoredProcedure;
                        com.CommandText = "dbo.PromoteStudents";
                        //kod tworzący procedurę PromoteStudents poniżej  
                        com.Parameters.Add(new SqlParameter("@Studies", request.Studies));
                        com.Parameters.Add(new SqlParameter("@Semester", request.Semester));

                        com.ExecuteNonQuery();

                        tran.Commit();

                        var enrollment = new PromoteStudentsResponse();                      
                        enrollment.Studies = request.Studies;
                        enrollment.Semester = (int)request.Semester + 1;

                        return new Response("201", "Promocja studentow ukonczona", enrollment);

                    }
                    catch (SqlException exc)
                    {
                        tran.Rollback();
                    }


                }

                 return new Response("400", null, null);

            }

        public bool StudentExists(string IndexNumber)
        {
            using (var con = new SqlConnection(ConString))
            using (var com = new SqlCommand())
            {
                com.Connection = con;
                con.Open();
                com.CommandText = @"select IndexNumber 
                                   from Student 
                                   where IndexNumber = @IndexNumber";
                com.Parameters.AddWithValue("IndexNumber", IndexNumber);

                var dr = com.ExecuteReader();
                if (!dr.Read())
                {
                    dr.Close();
                    return false;
                }
                               
                return true;
                
            }
        }

    }
}


/*
create procedure PromoteStudents @Studies NVARCHAR(100), @Semester INT
as
begin
    set xact_abort on;
begin tran


    declare @idStudy int = (select idStudy

                            from Studies
                            where name like @Studies);
							
	declare @idEnrollment int = (select idEnrollment
                                from enrollment
                                where idStudy = @idStudy and semester = @Semester); 

	declare @idEnrollmentPlus int = (select idEnrollment
                            from enrollment
                            where idStudy = @idStudy and semester = @Semester + 1); 
	
	if(@idEnrollmentPlus is null)
	begin
        set @idEnrollmentPlus = (select max(idEnrollment) from Enrollment) + 1;
		insert into enrollment
        values(@idEnrollmentPlus, @semester + 1, @idStudy, getDate());
end

update student
    set IdEnrollment = @idEnrollmentPlus
    where IdEnrollment = @idEnrollment;

	commit
end;
*/






