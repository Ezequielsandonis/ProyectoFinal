

using System.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TucConnect.Data;
using TucConnect.Data.Enums;
using TucConnect.Models;


namespace TucConnect.Data.Servicios
{
    public class PostServicio  // clase para recuperar informacion de posts desde la db
    {
        private readonly Contexto _contexto;   //objeto para la Conexion a la base de datos 

        public PostServicio(Contexto con) // la clase se inicializa con una conexion a la bdd y servidor
        {
            _contexto = con;



        }

        // **CADA METODO DE ESTA CLASE DE SERVICIO RECUPERA INFORMACION USANDO UNA CONSULTA SQL LLAMANDO  A UN PROCEDIMIENTO ALMACENADO** //



        //POST POR ID---s3
        public Post ObtenerPostPorId(int id)
        {
            var post = new Post(); //inicializar el modelo
            using (var connection = new SqlConnection(_contexto.Conexion))
            {
                connection.Open(); //abrir conexion
                using (var command = new SqlCommand("ObtenerPostPorId", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PostId", id);
                    using (var reader = command.ExecuteReader()) // recorrer el reader y ejecutarlo para llenarlo
                    {
                        if (reader.Read()) //validar que el reader "lea"
                        {
                            post = new Post //crear objeto de tipo post  con los datos necesarios 
                            {
                                PostId = (int)reader["PostId"], //convertir a tipo int 
                                Titulo = (string)reader["Titulo"],//convertir a tipo string 
                                Contenido = (string)reader["Contenido"],
                                Categoria = (CategoriaEnum)Enum.Parse(typeof(CategoriaEnum), (string)reader["Categoria"]),
                                Zona = (ZonaEnum)Enum.Parse(typeof(ZonaEnum), (string)reader["Zona"]),
                                FechaCreacion = (DateTime)reader["FechaCreacion"],
                                UsuarioId = (int)reader["UsuarioId"], //convertir a tipo int 

                            };

                        }

                        reader.Close();
                    }
                }
            }
            return post;
        }



        //OBTENER POST  --s2

        public List<Post> ObtenerPosts() //metodo de tipo lista
        {

            var posts = new List<Post>();

            using (var connection = new SqlConnection(_contexto.Conexion))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand("ObtenerTodosLosPost", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())//ejecutar 
                    {
                        while (reader.Read()) //mientras el reader, lea
                        {
                            var post = new Post  //crear objeto de tipo post  con los datos necesarios 
                            {
                                PostId = (int)reader["PostId"], //convertir a tipo int 
                                Titulo = (string)reader["Titulo"],//convertir a tipo string 
                                Contenido = (string)reader["Contenido"],
                                Categoria = (CategoriaEnum)Enum.Parse(typeof(CategoriaEnum), (string)reader["Categoria"]),
                                Zona = (ZonaEnum)Enum.Parse(typeof(ZonaEnum), (string)reader["Zona"]),
                                FechaCreacion = (DateTime)reader["FechaCreacion"],
                                UsuarioId = (int)reader["UsuarioId"], //convertir a tipo int 
                            };
                            posts.Add(post); // agregarlos a la lista
                        }

                    }
                }
            }


            return posts;
        }


        // OBTENER NOMBRE DEL CREADOR DEL POST
       

        public string ObtenerNombreUsuarioPorPostId(int postId)
        {
            string nombreUsuario = null;

            using (var connection = new SqlConnection(_contexto.Conexion))
            {
                connection.Open();

                using (var command = new SqlCommand("ObtenerNombreUsuarioPorPostId", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PostId", postId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            nombreUsuario = reader["NombreUsuario"].ToString();
                        }
                    }
                }
            }

            return nombreUsuario;
        }

        //LISTAR POST POR USUARIO ID 
        //obgtener id de usuario autenticado




        public List<Post> ListarPostPorUsuarioId(int userId) // método de tipo lista
        {
            var posts = new List<Post>();

            using (var connection = new SqlConnection(_contexto.Conexion))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand("ListarPostPorUsuarioId", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UsuarioId", userId);
                    using (var reader = cmd.ExecuteReader()) // ejecutar
                    {
                        while (reader.Read()) // mientras el reader, lea
                        {
                            var post = new Post // crear objeto de tipo post con los datos necesarios
                            {
                                PostId = (int)reader["PostId"], // convertir a tipo int
                                Titulo = (string)reader["Titulo"], // convertir a tipo string
                                Contenido = (string)reader["Contenido"],
                                Categoria = (CategoriaEnum)Enum.Parse(typeof(CategoriaEnum), (string)reader["Categoria"]),
                                Zona = (ZonaEnum)Enum.Parse(typeof(ZonaEnum), (string)reader["Zona"]),
                                FechaCreacion = (DateTime)reader["FechaCreacion"],
                                UsuarioId = (int)reader["UsuarioId"], // convertir a tipo int
                            };
                            posts.Add(post); // agregarlos a la lista
                        }
                    }
                }
            }
            return posts;
        }





        //OBTENER POST POR CATEGORIA

        public List<Post> ObtenerPostsPorCategoria(CategoriaEnum categoria) //metodo de tipo lista
        {

            var posts = new List<Post>();
            using (var connection = new SqlConnection(_contexto.Conexion))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand("ObtenerPostPorCategoria", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Categoria", categoria.ToString()); //agregar parametro
                    using (var reader = cmd.ExecuteReader())//ejecutar 
                    {
                        while (reader.Read())
                        {
                            var post = new Post //crear objeto de tipo post  con los datos necesarios 
                            {
                                PostId = (int)reader["PostId"], //convertir a tipo int 
                                Titulo = (string)reader["Titulo"],//convertir a tipo string 
                                Contenido = (string)reader["Contenido"],
                                Categoria = (CategoriaEnum)Enum.Parse(typeof(CategoriaEnum), (string)reader["Categoria"]),
                                Zona = (ZonaEnum)Enum.Parse(typeof(ZonaEnum), (string)reader["Zona"]),
                                FechaCreacion = (DateTime)reader["FechaCreacion"],
                                UsuarioId = (int)reader["UsuarioId"], //convertir a tipo int 
                            };
                            posts.Add(post);
                        }
                    }
                }
            }
            return posts;
        }

        //POST POR TITULO

        public List<Post> ObtenerPostsPorTitulo(string titulo) //metodo de tipo lista
        {

            var posts = new List<Post>();
            using (var connection = new SqlConnection(_contexto.Conexion))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand("ObtenerPostPorTitulo", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Titulo", titulo); //agregar parametro
                    using (var reader = cmd.ExecuteReader())//ejecutar 
                    {
                        while (reader.Read())
                        {
                            posts.Add(new Post //crear objeto/lista de tipo post  con los datos necesarios y agregarlos
                            {
                                PostId = (int)reader["PostId"], //convertir a tipo int 
                                Titulo = (string)reader["Titulo"],//convertir a tipo string 
                                Contenido = (string)reader["Contenido"],
                                Categoria = (CategoriaEnum)Enum.Parse(typeof(CategoriaEnum), (string)reader["Categoria"]),
                                Zona = (ZonaEnum)Enum.Parse(typeof(ZonaEnum), (string)reader["Zona"]),
                                FechaCreacion = (DateTime)reader["FechaCreacion"],
                                UsuarioId = (int)reader["UsuarioId"], //convertir a tipo int 
                            });

                        }
                    }
                }
            }


            return posts;
        }


        //POST POR  ZONA

        public List<Post> ObtenerPostsPorZona(ZonaEnum zona) //metodo de tipo lista
        {

            var posts = new List<Post>();
            using (var connection = new SqlConnection(_contexto.Conexion))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand("ObtenerPostPorZona", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Zona", zona.ToString()); //agregar parametro
                    using (var reader = cmd.ExecuteReader())//ejecutar 
                    {
                        while (reader.Read())
                        {
                            var post = new Post //crear objeto de tipo post  con los datos necesarios 
                            {
                                PostId = (int)reader["PostId"], //convertir a tipo int 
                                Titulo = (string)reader["Titulo"],//convertir a tipo string 
                                Contenido = (string)reader["Contenido"],
                                Categoria = (CategoriaEnum)Enum.Parse(typeof(CategoriaEnum), (string)reader["Categoria"]),
                                FechaCreacion = (DateTime)reader["FechaCreacion"],
                                Zona = (ZonaEnum)Enum.Parse(typeof(ZonaEnum), (string)reader["Zona"]),
                                UsuarioId = (int)reader["UsuarioId"], //convertir a tipo int 
                            };
                            posts.Add(post);
                        }
                    }
                }
            }
            using (var connection = new SqlConnection(_contexto.Conexion))

            return posts;
        }



        // COMENTARIOS POR ID  s4 

        // COMENTARIOS HIJOS s4


        //COMENTARIOS NIETOS s4




    }
}

