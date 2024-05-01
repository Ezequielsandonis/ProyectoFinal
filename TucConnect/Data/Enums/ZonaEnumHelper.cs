using System.ComponentModel;
using System.Reflection;

namespace TucConnect.Data.Enums
{
    public class ZonaEnumHelper
    {
        //OBTENER  LA DESCRIPCION
        public static string ObtenerDescripcionZona(ZonaEnum zona) //metodo estatico 
        {
            //se utiliza  reflection para obtener la descripcion 
            //se busca y obtiene el atributo en  el campo
            FieldInfo? field = zona.GetType().GetField(zona.ToString());


            //si se encuentra el atributo se devuelve la descripcion de la categoria
            DescriptionAttribute? attribute = field?.GetCustomAttribute<DescriptionAttribute>();

            //si el atributo no existe en el campo
            return attribute != null ? attribute.Description : zona.ToString(); //se devuelve el nombre de la zona (si es nulo)  
        }
    }
}
