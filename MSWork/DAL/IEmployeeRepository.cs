using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSWork.Models;

namespace MSWork.DAL
{
    interface IEmployeeRepository
    {
        IList<Employee> GetAll();

        IList<Employee> Filter(string firstName, string lastName, int? reportsTo, DateTime? birthDate,
                                int page, int pageSize, string sortField, bool sortDesc, out int totalCount);
        void Insert(Employee emp);

        Employee GetById(int id);

        void Update(Employee emp);

        int DeleteWithStoredPro(int id);
    }
}
