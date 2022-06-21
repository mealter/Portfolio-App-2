using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace MSWork.Models
{
    public class Employee
    {
        public int? EmployeeId { get; set; }

        [DisplayName("First Name")]

        public string FirstName { get; set; }

        [DisplayName("Last Name")]

        public string LastName { get; set; }

        public string Title { get; set; }

        [DisplayName("Reports To")]
        public int? ReportsTo { get; set; }

        [DisplayName("Birth Date")]

        public Employee ReportsToEmployee { get; set; }

        public DateTime? BirthDate { get; set; }

        public byte[] Photo { get; set; } 
    }
}