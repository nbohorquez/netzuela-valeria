namespace Zuliaworks.Netzuela.Valeria.Servidor.Api
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Runtime.Serialization;
    using System.Web;
    using System.Web.SessionState;
    
    public class Global : HttpApplication
    {        
        #region Implementacion de interfaces

        protected void Application_Start(object sender, EventArgs e)
        {
            var anfitrion = new Anfitrion();
            anfitrion.Init();
            //anfitrion.Start("http://serverhost:81"); //Starts HttpListener listening on 81
        }

        #endregion
    }
}