using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.Auth
{
    public class CreateRoleRequest
    {
        public string RoleName { get; set; } = default!;
    }
}
