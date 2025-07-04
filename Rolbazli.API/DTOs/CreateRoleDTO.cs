﻿using System.ComponentModel.DataAnnotations;

namespace Rolbazli.API.DTOs
{
    public class CreateRoleDTO
    {
        [Required(ErrorMessage = "Role ismi zorunlu.")]
        public string Name { get; set; } = null;
    }
}
