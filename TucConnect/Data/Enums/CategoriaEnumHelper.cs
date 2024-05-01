using System.ComponentModel;
using System.Reflection;

namespace TucConnect.Data.Enums
{
    public class CategoriaEnumHelper
    {
        //OBTENER  LA DESCRIPCION
        public static string ObtenerDescripcion(CategoriaEnum categoria) //metodo estatico 
        {
            //se utiliza  reflection para obtener la descripcion 
            //se busca y obtiene el atributo en  el campo
            FieldInfo? field = categoria.GetType().GetField(categoria.ToString());


            //si se encuentra el atributo se devuelve la descripcion de la categoria
            DescriptionAttribute? attribute = field?.GetCustomAttribute<DescriptionAttribute>();

            //si el atributo no existe en el campo
            return attribute != null ? attribute.Description : categoria.ToString(); //se devuelve el nombre de la categoria (si es nulo)  
        }
    }
}
