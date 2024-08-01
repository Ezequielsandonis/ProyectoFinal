using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TucConnect.Models
{
    public class Comentario
    {
        public int ComentarioId { get; set; }

        [Required(ErrorMessage = "El Contenido es requerido")]
        [StringLength(5000, MinimumLength = 5, ErrorMessage = "El Contenido debe tener entre 5 y 5000 caracteres")]
        public string? Contenido { get; set; }

        public DateTime FechaCreacion { get; set; }
        public int UsuarioId { get; set; }
        public int PostId { get; set; }

        public int? ComentarioPadreId { get; set; } // comentario principal

        public List<Comentario>? ComentariosHijos { get; set; } //Respuestas al comentario principal ( comentarios Hijos) -- EN forma de lista

        [NotMapped] // propiedades que no estan mapeadas  en la base de datos
        public string? NombreUsuario { get; set; } // representa el nombre del autor del comentario
        public int? ComentarioAbueloId { get; set; } // Comentarios enlazados a comentarios padres y comentarios hijos 
    }
}
