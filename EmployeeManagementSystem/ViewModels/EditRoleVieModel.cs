using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagementSystem.ViewModels
{
    public class EditRoleVieModel
    {
        public EditRoleVieModel()
        {
            UserNames = new List<string>();
        }
        public string Id { get; set; }
        [Required(ErrorMessage ="Role Name Is Required")]
        public string Name { get; set; }

        public List<string> UserNames { get; set; }
    }
}
