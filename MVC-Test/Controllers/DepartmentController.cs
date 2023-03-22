﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Group6Application.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using System.Net;
using System.Data.SqlClient;
using System.Data;
using Npgsql;
using MVC_Test.Models;
using System.Threading.Tasks;
using System.Xml;


namespace Group6Application.Controllers
{

    public class DepartmentController : Controller
    {
        private static string _connectionString = "Server=20.150.147.106;Port=5432;Database=Group6-PMS;User Id=postgres;Password=KHf37p@&R2hf2l";

        [Route("Department")]
        public ActionResult Index()
        {
            string viewPath = "Views/Department/Index.cshtml";
            
            DepartmentView viewModel = new DepartmentView()
            {
                Departments = new List<DepartmentTemplate>()
            };
            // SQL
            string sqlQuery = $"SELECT * FROM \"Department\";";
            NpgsqlConnection conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            using (NpgsqlCommand command = new NpgsqlCommand("", conn))
            {
                try
                {
                    command.CommandText = sqlQuery.ToString();
                    NpgsqlDataReader dataReader = command.ExecuteReader();
                    while (dataReader.Read())
                    {
                        DepartmentTemplate dept = new()
                        {
                            ID = (int)dataReader["ID"],
                            Name = dataReader["Name"].ToString(),
                            Number_of_Employees = (int)dataReader["Number_of_Employees"],
                        };
                        // possible null values
                        if (dataReader["SupervisorID"] != null )
                        {
                            dept.SupervisorID = dataReader["SupervisorID"].ToString();
                        }

                        viewModel.Departments.Add(dept);
                    }
                }
                catch
                {
                    // error catch here
                }
                finally
                {
                    conn.Close();
                }
                
            }

            return View(viewPath, viewModel);
        }

        [Route("Department/Add")]

        public ActionResult AddDepartment()
        {
            /*
            string userRole = Request.Cookies["Name"].Value;
            if (userRole != "Manager" && userRole != "Supervisor") // only Manager and Supervisor can add a department, the rest get an error
            {
                bool submissionResult = false;
                string errorMessage = "User does not have permission to view this page";
                return Json(new { submissionResult = submissionResult, message = errorMessage });
            }
            */
            string viewPath = "Views/Department/AddDepartment.cshtml";
            DepartmentView viewModel = new();
            List<SelectListItem> supervisorIDs = new List<SelectListItem>();

            
             foreach( DataRow row in Data.SupervisorIDs().Rows)
             {
                supervisorIDs.Add(new SelectListItem() { Value =row["ID"].ToString(), Text = row["Name"].ToString() });
             }
             
            viewModel.SupervisorIDs = supervisorIDs;

            return PartialView(viewPath, viewModel);
        }

        public ActionResult AddDepartmentDB(string Name, string SupervisorID)
        {
            bool submissionResult = false;
            string errorMessage = "";

            // SQL
            string sqlQuery = $"INSERT INTO \"Department\"(\"Name\",\"Number_of_Employees\",\"SupervisorID\") VALUES (@Name,@Number_of_Employees,@SupervisorID);";
            using (NpgsqlConnection conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand("", conn);
                NpgsqlTransaction sqlTransaction;
                sqlTransaction = conn.BeginTransaction();
                command.Transaction = sqlTransaction;

                try
                {
                    command.CommandText = sqlQuery.ToString();
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@Name", Name);
                    command.Parameters.AddWithValue("@Number_of_Employees", 0);
                    // Supervisor ID can be NULL, if it is replace it with DBnull
                    command.Parameters.AddWithValue("@SupervisorID", (string.IsNullOrEmpty(SupervisorID)) ? (object)DBNull.Value : SupervisorID);
                    command.ExecuteScalar(); // Automatically creates primary key, must set constraint on primary key to "Identity"
                    
                    sqlTransaction.Commit();
                    submissionResult = true;
                }
                catch (Exception e)
                {
                    // error catch here
                    errorMessage = e.ToString();
                    sqlTransaction.Rollback();
                    //errorMessage = "We experienced an error while adding to database";
                    Console.WriteLine(e.ToString());
                }
                finally
                {
                    conn.Close();
                }
            };

            return Json(new { submissionResult = submissionResult, message = errorMessage });
        }

    }
}
