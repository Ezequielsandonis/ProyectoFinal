

using System.ComponentModel;

namespace TucConnect.Data.Enums //En los enums se definen constantes que se usan junto con la base de datos, en este caso las categorias
{
    public enum CategoriaEnum
    {
        [Description("Noticias recientes")]
        Noticias,
        [Description("Ofertas laborales")]
        Trabajo,
        [Description("Lo que buscas al alcance de tus manos")]
        Productos,
        [Description("Tutoriales")]
        Tutoriales,
        [Description("Recursos útiles")]
        Recursos,
        [Description("Servicios")]
        Servicios,
        [Description("Otros")]
        Otro,




    }
}
