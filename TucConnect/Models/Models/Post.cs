using System.ComponentModel.DataAnnotations;
using TucConnect.Data.Enums;

namespace TucConnect.Models
{
    public class Post
    {
        public int PostId { get; set; }

        [Required(ErrorMessage = "El titulo es requerido")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "El titulo debe tener entre 5 y 100 caracteres")]
        public string? Titulo { get; set; }

        [Required(ErrorMessage = "El Contenido es requerido")]
        [StringLength(5000, MinimumLength = 5, ErrorMessage = "El Contenido debe tener entre 5 y 5000 caracteres")]
        public string? Contenido { get; set; }

        [Required(ErrorMessage = "la categoria es requerido")]
        public CategoriaEnum Categoria { get; set; }

        [Required(ErrorMessage = "la zona es requerido")]
        public ZonaEnum Zona { get; set; }
        public DateTime FechaCreacion { get; set; }

        public int UsuarioId { get; set; }
    }
}
