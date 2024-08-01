using System.ComponentModel.DataAnnotations;

namespace TucConnect.Models
{
    public class Usuario
    {
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "El campo nombre debe tener maximo (1) 50 caracteres.")]
        public string? Nombre { get; set; }


        [Required(ErrorMessage = "El Apellido es obligatorio.")]
        [StringLength(50, ErrorMessage = "El campo Apellido debe tener maximo (1) 50 caracteres.")]
        public string? Apellido { get; set; }


        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "El correo debe ser valido")]
        [StringLength(100, ErrorMessage = "El campo nombre debe tener maximo (1) 100 caracteres.")]
        public string? Correo { get; set; }



        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "El campo contraseña debe tener maximo (1) 100 caracteres.")]
        public string Contrasenia { get; set; }

        [Required(ErrorMessage = "El campo rol id es obligatorio")]
        public int RolId { get; set; }


        public Rol? Rol { get; set; }
        [Required(ErrorMessage = "El Nombre de usuario es obligatorio.")]
        [StringLength(50, ErrorMessage = "El campo nombre de usuario debe tener maximo (1) 50 caracteres.")]
        public string? NombreUsuario { get; set; }
        public bool Estado { get; set; }
        public string? Token { get; set; }



        public DateTime? FechaExpiracion { get; set; }

    }
}
