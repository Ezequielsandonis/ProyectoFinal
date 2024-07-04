using Microsoft.AspNetCore.Mvc;
using X.PagedList;
using Microsoft.Extensions.Hosting;
using TucConnect.Data.Enums;
using TucConnect.Data.Servicios;
using TucConnect.Data;
using TucConnect.Models;



namespace MyBlog.Controllers
{


    public class HomeController : Controller
    {
        //instancias 
        private readonly Contexto _contexto;
        private readonly PostServicio _postServicio; //Se usa postServicios porque la vista index usa los posts

        //constructor
        public HomeController(Contexto contexto)
        {
            _contexto = contexto;
            _postServicio = new PostServicio(contexto);

        }


        public IActionResult Error()
        {
            return View();
        }

        public IActionResult Index(string categoria, string buscar, string zona, int? pagina)
        {
            //control de errores
            try
            {
                var post = new List<Post>();

                //validar que no se haya filtrado ni buscado nada
                if (string.IsNullOrEmpty(categoria) && string.IsNullOrEmpty(buscar) && string.IsNullOrEmpty(zona))

                    post = _postServicio.ObtenerPosts();//si no hay filtro se llama a este metodo
                                                        //asegurar que la categoria que se recibe no sea nula

                else if (!string.IsNullOrEmpty(categoria))
                {
                    // si se encuentra nun filtro:
                    var categoriaEnum = Enum.Parse<CategoriaEnum>(categoria);
                    // llamar al metodo para filtrar por categorias del servicio
                    post = _postServicio.ObtenerPostsPorCategoria(categoriaEnum);
                    //validar si hay posts
                    if (post.Count == 0)
                    {
                        ViewBag.Error = $"No hay publicaciones en la categoría {categoriaEnum}.";
                    }
                }



                else if (!string.IsNullOrEmpty(zona))
                {
                    // si se encuentra nun filtro:
                    var zonaEnum = Enum.Parse<ZonaEnum>(zona);
                    // llamar al metodo para filtrar por categorias del servicio
                    post = _postServicio.ObtenerPostsPorZona(zonaEnum);
                    //validar si hay posts
                    if (post.Count == 0)
                    {
                        ViewBag.Error = $"No hay publicaciones en la zona {zona}.";
                    }
                }


                //validar  que no sea nulo el buscar
                else if (!string.IsNullOrEmpty(buscar))
                {
                    //llamar al metodo Obtener el post por titulo y zona del servicio de posteos
                    post = _postServicio.ObtenerPostsPorTitulo(buscar);

                    if (post.Count == 0)
                    {
                        ViewBag.Error = $"No hay publicaciones sobre: {buscar}.";
                    }
                }

                //paginacion
                int pageSize = 6;
                //si no hay nada en la variable Numbre , se asigna el 1
                int pageNumber = (pagina ?? 1);

                //condicion de que si categoria sea distinto a nulo se carga el metodo de la clase CategoriaEnumHelper al cual se le envia la categoria
                // si es nulo se muestra un mensaje
                string descripcionCategoria = !string.IsNullOrEmpty(categoria) ?
                    CategoriaEnumHelper.ObtenerDescripcion(Enum.Parse<CategoriaEnum>(categoria)) : "Todas las categorias";
                // Crear el objeto JSON con la descripción de la categoría
                ViewBag.CategoriaDescripcion = descripcionCategoria;

                string descripcionZona = !string.IsNullOrEmpty(zona) ?
                   ZonaEnumHelper.ObtenerDescripcionZona(Enum.Parse<ZonaEnum>(zona)) : "Todas las zonas";

                // Crear el objeto JSON con la descripción de la categoría
                ViewBag.DescripcionZona = descripcionZona;

                //paginar resultados con un limite de paginas (6) usando el metodo "ToPagedList" de (X.PagedList;)
                return View(post.ToPagedList(pageNumber, pageSize));

            }
            catch (Exception ex)
            {

                var err = new
                {
                    error = ex.Message
                };

                ViewBag.Error = ex.Message;
                return View();
            }

        }




    }


}
