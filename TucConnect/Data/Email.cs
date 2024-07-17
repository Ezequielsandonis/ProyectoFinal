using System.Net.Mail;
using System.Net;

namespace TucConnect.Data
{
    public class Email
    {
        //METODO para aislar el metodo de envio de correo con el metodo que contiene la informacion  del correo
        //este metodo solo llama al metodo Correo 
        //este metodo se llama a mandar desde cualquier otra clase
        public void Enviar(string correo, string token)
        {
            Correo(correo, token);
        }


        //datos con la informacion  del correo
        //este metodo se encarga de llenar y mandar el correo
        void Correo(string correo_receptor, string token)
        {
            string correo_emisor = "tucconectado@gmail.com";
            string clave_emisor = "hkdc nypf npbf yooj";

            MailAddress receptor = new(correo_receptor);
            MailAddress emisor = new(correo_emisor);

            MailMessage email = new(emisor, receptor);
            email.Subject = "Tucumán conectado:Activación de cuentas";
            email.Body = @" <!DOCTYPE html>
                    <html> 
                      <head> https://www.tucconnect.somee.com/
                        <title>Active su cuenta</title>
                      </head>
                      <body><h2>Para activar su cuenta, ingrese al siguiente enlace:</h2>
                      <a href='https://www.tucconnect.somee.com/Cuenta/Token?valor=" + token + "'>Activar Cuenta</a></body></html>";
            email.IsBodyHtml = true; //definir que el cuerpo tiene estructura html para darle un diseño

            SmtpClient smtp = new();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;  //465
            smtp.Credentials = new NetworkCredential(correo_emisor, clave_emisor);
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.EnableSsl = true;

            //control de errores en el envio https://www.tucconnect.somee.com/Cuenta/Token?valor="+ token +;
            try
            {
                //enviar em correo
                smtp.Send(email);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void EnviarNotificacion(string correo, string channelUrl)
        {
            EnviarCorreoPropietario(correo, channelUrl);
        }

         void EnviarCorreoPropietario(string correo, string channelUrl)
        {
            // Construir el cuerpo del correo con el link del canal de chat
            string correo_emisor = "tucconectado@gmail.com";
            string clave_emisor = "hkdc nypf npbf yooj";

            MailAddress receptor = new(correo);
            MailAddress emisor = new(correo_emisor);

            MailMessage email = new(emisor, receptor);
            email.Subject = "Tienes un nuevo chat esperandote";
            email.Body = @" <!DOCTYPE html>
                    <html> 
                      <head> https://www.tucconnect.somee.com/
                        <title>Nuevo chat iniciado</title>
                      </head>
                      <body><h2>Se ha iniciado un nuevo chat en la plataforma. Revisa tus chats para encontrarlo.</h2>";
            email.IsBodyHtml = true; // Definir que el cuerpo tiene estructura HTML para darle un diseño

            SmtpClient smtp = new();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587; //465
            smtp.Credentials = new NetworkCredential(correo_emisor, clave_emisor);
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.EnableSsl = true;

            try
            {
                // Enviar el correo
                smtp.Send(email);
            }
            catch (Exception ex)
            {
                // Manejo de errores si falla el envío del correo
                throw new Exception("Error al enviar el correo electrónico al propietario del post.", ex);
            }
        }

    }




}
