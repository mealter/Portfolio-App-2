using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using MSWork.Models;

namespace MSWork.DAL
{
	public class EmployeeRepository : IEmployeeRepository
	{
		private string ConnStr
        {
			get 
			{
				return WebConfigurationManager.ConnectionStrings["MSConnString"].ConnectionString;
			}
			
        }

        public int DeleteWithStoredPro(int id)
        {
            using (var conn = new SqlConnection(ConnStr))
            {
                using (var cmd =  conn.CreateCommand())
                {
					cmd.CommandText = @"udpDelEmployee";
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.AddWithValue("id", id);
					var pErr = cmd.CreateParameter();
					pErr.ParameterName = "err";
					pErr.Direction = ParameterDirection.Output;
					pErr.SqlDbType = SqlDbType.VarChar;
					pErr.Size = 1000;
					cmd.Parameters.Add(pErr);
					var pRet = cmd.CreateParameter();
					pRet.Direction = ParameterDirection.ReturnValue;
					cmd.Parameters.Add(pRet);

					conn.Open();
					cmd.ExecuteNonQuery();
					int retVal = (int)pRet.Value;
					string err = (string)pErr.Value;

					if (retVal < 0)
					{
						throw new Exception(err);
					}
					else {
						return retVal;
					}
                }
            }
        }

        public IList<Employee> Filter(string firstName, string lastName, int? reportsTo, DateTime? birthDate,
										int page, int pageSize, string sortField, bool sortDesc, out int totalCount)
        {
			IList<Employee> employees = new List<Employee>();
			using (var conn = new SqlConnection(ConnStr))
			{
				using (var cmd = conn.CreateCommand())
				{
					string fields =		@"	e.EmployeeId,
											e.FirstName,
											e.LastName,
											e.Title,
											e.ReportsTo,
											e.BirthDate,
											s.FirstName as SFirstName,
											s.LastName as SLastName,
											s.Title as STitle ";
					string sql = @"SELECT
											{0}
									FROM Employee e LEFT JOIN Employee s on e.ReportsTo = s.EmployeeId
									";


					string whereSql = "";
                    if (!string.IsNullOrWhiteSpace(firstName))
                    {
						whereSql += (whereSql.Length == 0 ? "" : " AND " )
									+ " e.FirstName like @FirstName + '%' ";
						cmd.Parameters.AddWithValue("@FirstName", firstName);
                    }

					if (!string.IsNullOrWhiteSpace(lastName))
					{
						whereSql += (whereSql.Length == 0 ? "" : " AND ") 
									+ " e.LastName like @LastName + '%' ";
						cmd.Parameters.AddWithValue("@LastName", lastName);
					}

					if (reportsTo.HasValue)
					{
						whereSql += (whereSql.Length == 0 ? "" : " AND ")
									+ " e.ReportsTo = @ReportsTo ";
						cmd.Parameters.AddWithValue("@ReportsTo", reportsTo);
					}

					if (birthDate.HasValue)
					{
						whereSql += (whereSql.Length == 0 ? "" : " AND ") + " e.BirthDate <= @BirthDate ";
						cmd.Parameters.AddWithValue("@BirthDate", birthDate);
					}

                    if (!string.IsNullOrWhiteSpace(whereSql))
                    {
						whereSql =  " WHERE " + whereSql;
                    }

					conn.Open();
					cmd.CommandText =string.Format(sql," count(*) ") + whereSql;
					totalCount = (int)cmd.ExecuteScalar();

					string pageSql = " OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";
					cmd.Parameters.AddWithValue("@offset", (page - 1) * pageSize);
					cmd.Parameters.AddWithValue("@pageSize", pageSize);

					if (string.IsNullOrWhiteSpace(sortField))
						sortField = "FirstName";

					cmd.CommandText = string.Format(sql,fields) + whereSql 
							+ $" ORDER BY {sortField} {(sortDesc ? "DESC" : "ASC")} " + pageSql;

					

					using (var rdr = cmd.ExecuteReader())
					{
						while (rdr.Read())
						{
							var emp = new Employee();
							emp.EmployeeId = rdr.GetInt32(rdr.GetOrdinal("EmployeeId"));
							emp.FirstName = rdr.GetString(rdr.GetOrdinal("FirstName"));
							emp.LastName = rdr.GetString(rdr.GetOrdinal("LastName"));
							emp.Title = rdr.GetString(rdr.GetOrdinal("Title"));
							if (!rdr.IsDBNull(rdr.GetOrdinal("ReportsTo")))
								emp.ReportsTo = rdr.GetInt32(rdr.GetOrdinal("ReportsTo"));
							if (!rdr.IsDBNull(rdr.GetOrdinal("BirthDate")))
								emp.BirthDate = rdr.GetDateTime(rdr.GetOrdinal("BirthDate"));
							emp.ReportsToEmployee = new Employee();
							if (!rdr.IsDBNull(rdr.GetOrdinal("SFirstName")))
								emp.ReportsToEmployee.FirstName = rdr.GetString(rdr.GetOrdinal("SFirstName"));
							if (!rdr.IsDBNull(rdr.GetOrdinal("SLastName")))
								emp.ReportsToEmployee.LastName = rdr.GetString(rdr.GetOrdinal("SLastName"));
							if (!rdr.IsDBNull(rdr.GetOrdinal("STitle")))
								emp.ReportsToEmployee.Title = rdr.GetString(rdr.GetOrdinal("STitle"));


							employees.Add(emp);

						}
					}
				}



			}
			return employees;
		}

        public IList<Employee> GetAll()
		{
			IList<Employee> employees = new List<Employee>();
			using (var conn = new SqlConnection(ConnStr))
			{
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = @"SELECT e.EmployeeId,
											e.FirstName,
											e.LastName,
											e.Title,
											e.ReportsTo,
											e.BirthDate,
											s.FirstName as SFirstName,
											s.LastName as SLastName,
											s.Title as STitle
									FROM Employee e LEFT JOIN Employee s on e.ReportsTo = s.EmployeeId";
					conn.Open();
					using (var rdr = cmd.ExecuteReader())
					{
                        while (rdr.Read())
                        {
							var emp = new Employee();
							emp.EmployeeId = rdr.GetInt32(rdr.GetOrdinal("EmployeeId"));
							emp.FirstName = rdr.GetString(rdr.GetOrdinal("FirstName"));
							emp.LastName = rdr.GetString(rdr.GetOrdinal("LastName"));
							emp.Title = rdr.GetString(rdr.GetOrdinal("Title"));
							if(!rdr.IsDBNull(rdr.GetOrdinal("ReportsTo")))
								emp.ReportsTo = rdr.GetInt32(rdr.GetOrdinal("ReportsTo"));
							if(!rdr.IsDBNull(rdr.GetOrdinal("BirthDate")))
								emp.BirthDate = rdr.GetDateTime(rdr.GetOrdinal("BirthDate"));
							emp.ReportsToEmployee = new Employee();
							if (!rdr.IsDBNull(rdr.GetOrdinal("SFirstName")))
								emp.ReportsToEmployee.FirstName = rdr.GetString(rdr.GetOrdinal("SFirstName"));
							if (!rdr.IsDBNull(rdr.GetOrdinal("SLastName")))
								emp.ReportsToEmployee.LastName = rdr.GetString(rdr.GetOrdinal("SLastName"));
							if (!rdr.IsDBNull(rdr.GetOrdinal("STitle")))
								emp.ReportsToEmployee.Title = rdr.GetString(rdr.GetOrdinal("STitle"));


							employees.Add(emp);

						}
					}
				}

				
				
			}
			return employees;
		}

        public Employee GetById(int id)
        {
			Employee employee = null;
			using (var conn = new SqlConnection(ConnStr))
            {
                using (var cmd = conn.CreateCommand())
                {
					cmd.CommandText = @"SELECT EmployeeId,
											FirstName,
											LastName,
											Title,
											ReportsTo,
											BirthDate,
											Photo
									from Employee
									WHERE EmployeeId = @EmployeeId";
					cmd.Parameters.AddWithValue(@"EmployeeId", id);

					conn.Open();
					using (var rdr = cmd.ExecuteReader())
					{
                        if (rdr.Read())
                        {
							employee = new Employee()
							{
								EmployeeId = id,
								FirstName = rdr.GetString(rdr.GetOrdinal("FirstName")),
								LastName = rdr.GetString(rdr.GetOrdinal("LastName")),
								Title = rdr.GetString(rdr.GetOrdinal("Title")),
								ReportsTo = !rdr.IsDBNull(rdr.GetOrdinal("ReportsTo"))
									? rdr.GetInt32(rdr.GetOrdinal("ReportsTo")) : (int?)null,
								BirthDate = !rdr.IsDBNull(rdr.GetOrdinal("BirthDate"))
									? rdr.GetDateTime(rdr.GetOrdinal("BirthDate")) : (DateTime?)null,
								Photo = !rdr.IsDBNull(rdr.GetOrdinal("Photo")) 
											? (byte[])rdr["Photo"] :null

							};
                        }
					}

					return employee;
				}
            }
		}

        public void Insert(Employee emp)
        {
            using (var conn = new SqlConnection(ConnStr))
            {
                using (var cmd = conn.CreateCommand())
                {
					cmd.CommandText = @"insert into	Employee(
											FirstName,
											LastName,
											Title,
											ReportsTo,
											BirthDate,
											Photo
											)
									values(
											@FirstName,
											@LastName,
											@Title,
											@ReportsTo,
											@BirthDate,
											@Photo
											)";

					var pLastName = cmd.CreateParameter();
					pLastName.ParameterName = "FirstName";
					pLastName.Value = emp.FirstName;
					pLastName.Direction = ParameterDirection.Input;
					pLastName.DbType = DbType.AnsiString;
					cmd.Parameters.Add(pLastName);

					cmd.Parameters.AddWithValue("LastName",emp.LastName);
					cmd.Parameters.AddWithValue("Title", emp.Title);
					if(emp.ReportsTo.HasValue)
						cmd.Parameters.AddWithValue("ReportsTo", emp.ReportsTo);
                    else
                    {
						cmd.Parameters.AddWithValue("ReportsTo", DBNull.Value);
					}
					cmd.Parameters.AddWithValue("BirthDate", (object)emp.BirthDate ?? DBNull.Value);
					cmd.Parameters.AddWithValue("Photo", (object)emp.Photo ?? SqlBinary.Null);

					conn.Open();

					cmd.ExecuteNonQuery();

				}
            }
        }

        public void Update(Employee emp)
        {
            using(var conn = new SqlConnection(ConnStr))
            {
				using(var cmd = conn.CreateCommand())
                {
					cmd.CommandText = @"UPDATE Employee
										SET 
											FirstName = @FirstName,
											LastName = @LastName,
											Title = @Title,
											ReportsTo = @ReportsTo,
											BirthDate = @BirthDate 
										WHERE EmployeeId = @EmployeeId";
					cmd.Parameters.AddWithValue("FirstName", emp.FirstName);

					cmd.Parameters.AddWithValue("@LastName", emp.LastName);
					cmd.Parameters.AddWithValue("@Title", emp.Title);
					cmd.Parameters.AddWithValue("@ReportsTo", (object)emp.ReportsTo ?? DBNull.Value);
					cmd.Parameters.AddWithValue("@BirthDate", (object)emp.BirthDate ?? DBNull.Value);
					cmd.Parameters.AddWithValue("@EmployeeId", emp.EmployeeId);
					conn.Open();
					cmd.ExecuteNonQuery();



				}
            }
        }
    }
}