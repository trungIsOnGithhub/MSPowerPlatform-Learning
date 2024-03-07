using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PAS.Repositories;
namespace PAS.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository _departmentRepository;
        public DepartmentService(IDepartmentRepository departmentRepository)
        {
            _departmentRepository = departmentRepository;
        }

        public int AddDepartment(Model.Domain.Department department)
        {
            int result = _departmentRepository.AddDepartment(department);
            if (result > 0) _departmentRepository.UnitOfWork.SaveEntities();
            return result;
        }

        public List<Model.Domain.Department> GetAllDepartments()
        {
            return _departmentRepository.GetAllDepartments();
        }
        public List<Model.Domain.Department> GetAllActiveDepartments()
        {
            return _departmentRepository.GetAllActiveDepartments();
        }
        public int UpdateDepartment(Model.Domain.Department department)
        {
            int result = _departmentRepository.UpdateDepartment(department);
            if (result > 0) _departmentRepository.UnitOfWork.SaveEntities();
            return result;
        }

        public List<Model.Domain.Department> GetDescendantDepartments(Model.Domain.Department department, List<Model.Domain.Department> departments)
        {
            var descendants = new List<Model.Domain.Department>();
            foreach (var item in department.Children)
            {
                var child = departments.Find(d => d.Id == item.Id);
                descendants.Add(child);
                if (child.Children?.Count > 0)
                {
                    descendants.AddRange(GetDescendantDepartments(child, departments));
                }
            }
            return descendants;
        }

        public bool ToggleDepartment(Model.Domain.Department department)
        {
            var result = (_departmentRepository.UpdateDepartmentStatus(department) != 0);
            var status = department.IsActive;

            var departments = _departmentRepository.GetAllDepartments();
            var departmentChildren = GetDescendantDepartments(department,departments);
            foreach(var item in departmentChildren)
            {
                item.IsActive = status;
                result = result && (_departmentRepository.UpdateDepartmentStatus(item)!=0);
            }
            if (result == true) _departmentRepository.UnitOfWork.SaveEntities();
            return result;
        }
    }
}
