using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAS.Services
{
    public interface IDepartmentService
    {
        List<Model.Domain.Department> GetAllDepartments();
        List<Model.Domain.Department> GetAllActiveDepartments();
        int AddDepartment(Model.Domain.Department department);
        int UpdateDepartment(Model.Domain.Department department);
        bool ToggleDepartment(Model.Domain.Department department);
        List<Model.Domain.Department> GetDescendantDepartments(Model.Domain.Department department, List<Model.Domain.Department> departments);
    }
}
