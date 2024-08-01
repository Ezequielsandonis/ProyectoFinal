namespace TucConnect.Models.ViewModels
{
    public class PostDetallesViewModel // este modelo se muestra en la vista de los detalles  de un post
    {
        public Post? Post { get; set; } // representa al post principal

        public string? UsuarioCreador { get; set; }

        public List<Comentario>? ComentariosPrincipales { get; set; } // lista de objetos  de tipo comentario que representa a los comentarios sin padre (comentarios principales) asociados a un post
        public List<Comentario>? ComentariosHijos { get; set; } // respuestas a los comentarios padre
        public List<Comentario>? ComentariosNietos { get; set; }   // respuestas a los comentarios hijos , su abuelo es el comentario principal

        public List<Post>? PostRecientes { get; set; }
    }
}
