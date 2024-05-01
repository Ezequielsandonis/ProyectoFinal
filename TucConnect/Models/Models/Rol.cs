using System.ComponentModel.DataAnnotations;

namespace TucConnect.Models
{
    public class Rol
    {
       public int RolId { get; set; }


        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "El campo nombre debe tener maximo (1) 50 caracteres.")]
        public string ? Nombre { get; set; }

        public ICollection<Usuario> ? Usuarios { get; set; } 
    }
}
