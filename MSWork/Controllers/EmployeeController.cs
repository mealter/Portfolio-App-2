using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MSWork.DAL;
using MSWork.Models;
using X.PagedList;

namespace MSWork.Controllers
{
    public class EmployeeController : Controller
    {
        // GET: Employee
        public ActionResult Index(string firstName, string lastName, int? reportsTo, DateTime? birthDate,
                        int? page, string sort)
        {
            int pageNumber = page ?? 1;
            int totalCount;
            int pageSize = 3;
            string sortField = "FirstName";
            bool sortDesc = false;
            if (!string.IsNullOrWhiteSpace(sort))
            {
                string[] arr = sort.Split('_');
                if (arr?.Length == 2)
                {
                    sortField = arr[0];
                    sortDesc = arr[1] == "DESC";
                }
            }


            var repository = new EmployeeRepository();
            var employees = repository.Filter(firstName, lastName, reportsTo, birthDate,
                            pageNumber, pageSize, sortField, sortDesc, out totalCount);
            var pagedList = new StaticPagedList<Employee>(employees, pageNumber, pageSize, totalCount);
            ViewBag.ReportsToList = repository.GetAll();
            ViewBag.FirstNameSort = sort == "FirstName_ASC" ? "FirstName_DESC" : "FirstName_ASC";
            ViewBag.LastNameSort = sort == "LastName_ASC" ? "LastName_DESC" : "LastName_ASC";
            ViewBag.CurrentPage = page;
            ViewBag.CurrentSort = sort;

            return View(pagedList);
        }

        // GET: Employee/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Employee/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Employee/Create
        [HttpPost]
        public ActionResult Create(Employee emp, HttpPostedFileBase imageFile)
        {
            var repository = new EmployeeRepository();
            
            try
            {
                if(imageFile?.ContentLength > 0)
                {
                    using (var stream = new MemoryStream())
                    {
                        imageFile.InputStream.CopyTo(stream);
                        emp.Photo = stream.ToArray();
                    }
                }
                repository.Insert(emp);
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }

        // GET: Employee/Edit/5
        public ActionResult Edit(int id)
        {
            var repository = new EmployeeRepository();
            var emp = repository.GetById(id);
            var supervisors = repository.GetAll();
            ViewBag.SupervisorsList = supervisors;
            return View(emp);
        }

        // POST: Employee/Edit/5
        [HttpPost]
        public ActionResult Edit(Employee emp)
        {
            var repository = new EmployeeRepository();

            try
            {
                repository.Update(emp);
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }

        // GET: Employee/Delete/5
        public ActionResult Delete(int id)
        {
            var repository = new EmployeeRepository();
            var emp = repository.GetById(id);
            return View(emp);
        }

        // POST: Employee/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, Employee employee)
        {
            try
            {
                var repo = new EmployeeRepository();
                repo.DeleteWithStoredPro(id);
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }

        public FileResult ShowImage(int id)
        {
            var repository = new EmployeeRepository();
            var emp = repository.GetById(id);
            if (emp != null && emp.Photo?.Length > 0 )
            {
                return File(emp.Photo, "image/jpeg", emp.FirstName + ".jpg");
            }
            else
            {
                return null;
            }
        }
    }
}
