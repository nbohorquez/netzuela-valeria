namespace Zuliaworks.Netzuela.Valeria.Datos.Web
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;                        // ProgressChangedEventArgs, AsyncOperation
    using System.Data;                                  // DataTable
    using System.Net;
    using System.Security;                              // SecureString
    using System.Threading;                             // Thread, SendOrPostCallback

    using ServiceStack.ServiceModel.Serialization;
    using Zuliaworks.Netzuela.Valeria.Datos.Eventos;
    using Zuliaworks.Netzuela.Valeria.Tipos;   			// DataSetXml

    /// <summary>
    /// 
    /// </summary>
    public partial class ClienteSpuria
    {
        #region Variables

        private SendOrPostCallback delegadoDispararCambioEnProgresoDeOperacion;
        private SendOrPostCallback delegadoDispararListarTiendasCompletado;
        private SendOrPostCallback delegadoDispararListarBDsCompletado;
        private SendOrPostCallback delegadoDispararListarTablasCompletado;
        private SendOrPostCallback delegadoDispararLeerTablaCompletado;
        private SendOrPostCallback delegadoDispararEscribirTablaCompletado;
        private SendOrPostCallback delegadoDispararCrearUsuarioCompletado;
        private SendOrPostCallback delegadoDispararConsultarCompletado;

        #endregion

        #region Eventos

        public event EventHandler<ProgressChangedEventArgs> CambioEnProgresoDeOperacion;
        public event EventHandler<EventoListarTiendasCompletadoArgs> ListarTiendasCompletado;
        public event EventHandler<EventoListarBDsCompletadoArgs> ListarBasesDeDatosCompletado;
        public event EventHandler<EventoListarTablasCompletadoArgs> ListarTablasCompletado;
        public event EventHandler<EventoLeerTablaCompletadoArgs> LeerTablaCompletado;
        public event EventHandler<EventoEscribirTablaCompletadoArgs> EscribirTablaCompletado;
        public event EventHandler<EventoCrearUsuarioCompletadoArgs> CrearUsuarioCompletado;
        public event EventHandler<EventoConsultarCompletadoArgs> ConsultarCompletado;

        #endregion

        #region Funciones

        #region Operaciones internas

        protected virtual void DispararCambioEnProgresoDeOperacion(ProgressChangedEventArgs e)
        {
            if (this.CambioEnProgresoDeOperacion != null)
            {
                this.CambioEnProgresoDeOperacion(this, e);
            }
        }

        protected virtual void DispararListarTiendasCompletado(EventoListarTiendasCompletadoArgs e)
        {
            if (this.ListarTiendasCompletado != null)
            {
                this.ListarTiendasCompletado(this, e);
            }
        }

        protected virtual void DispararListarBDsCompletado(EventoListarBDsCompletadoArgs e)
        {
            if (this.ListarBasesDeDatosCompletado != null)
            {
                this.ListarBasesDeDatosCompletado(this, e);
            }
        }

        protected virtual void DispararListarTablasCompletado(EventoListarTablasCompletadoArgs e)
        {
            if (this.ListarTablasCompletado != null)
            {
                this.ListarTablasCompletado(this, e);
            }
        }

        protected virtual void DispararLeerTablaCompletado(EventoLeerTablaCompletadoArgs e)
        {
            if (this.LeerTablaCompletado != null)
            {
                this.LeerTablaCompletado(this, e);
            }
        }

        protected virtual void DispararEscribirTablaCompletado(EventoEscribirTablaCompletadoArgs e)
        {
            if (this.EscribirTablaCompletado != null)
            {
                this.EscribirTablaCompletado(this, e);
            }
        }

        protected virtual void DispararCrearUsuarioCompletado(EventoCrearUsuarioCompletadoArgs e)
        {
            if (this.CrearUsuarioCompletado != null)
            {
                this.CrearUsuarioCompletado(this, e);
            }
        }

        protected virtual void DispararConsultarCompletado(EventoConsultarCompletadoArgs e)
        {
            if (this.ConsultarCompletado != null)
            {
                this.ConsultarCompletado(this, e);
            }
        }
        
        private void AntesDeDispararCambioEnProgresoDeOperacion(object estado)
        {
            ProgressChangedEventArgs e = (ProgressChangedEventArgs)estado;
            this.DispararCambioEnProgresoDeOperacion(e);
        }

        private void AntesDeDispararListarTiendasCompletado(object estadoOperacion)
        {
            EventoListarTiendasCompletadoArgs e = (EventoListarTiendasCompletadoArgs)estadoOperacion;
            this.DispararListarTiendasCompletado(e);
        }

        private void AntesDeDispararListarBDsCompletado(object estadoOperacion)
        {
            EventoListarBDsCompletadoArgs e = (EventoListarBDsCompletadoArgs)estadoOperacion;
            this.DispararListarBDsCompletado(e);
        }

        private void AntesDeDispararListarTablasCompletado(object estadoOperacion)
        {
            EventoListarTablasCompletadoArgs e = (EventoListarTablasCompletadoArgs)estadoOperacion;
            this.DispararListarTablasCompletado(e);
        }

        private void AntesDeDispararLeerTablaCompletado(object estadoOperacion)
        {
            EventoLeerTablaCompletadoArgs e = (EventoLeerTablaCompletadoArgs)estadoOperacion;
            this.DispararLeerTablaCompletado(e);
        }

        private void AntesDeDispararEscribirTablaCompletado(object estadoOperacion)
        {
            EventoEscribirTablaCompletadoArgs e = (EventoEscribirTablaCompletadoArgs)estadoOperacion;
            this.DispararEscribirTablaCompletado(e);
        }

        private void AntesDeDispararCrearUsuarioCompletado(object estadoOperacion)
        {
            EventoCrearUsuarioCompletadoArgs e = (EventoCrearUsuarioCompletadoArgs)estadoOperacion;
            this.DispararCrearUsuarioCompletado(e);
        }

        private void AntesDeDispararConsultarCompletado(object estadoOperacion)
        {
            EventoConsultarCompletadoArgs e = (EventoConsultarCompletadoArgs)estadoOperacion;
            this.DispararConsultarCompletado(e);
        }

        #endregion

        #region Métodos sincrónicos

        public string[] ListarTiendas()
        {
            /*
            string[] resultado = new string[] { };
            
            using (cliente = new JsonServiceClient(this.uriServidorJsonSync + "ListarTiendas"))
            {
                var peticion = new ListarTiendas();
                var respuesta = cliente.Send<ListarTiendasResponse>(peticion);

                if (respuesta.ResponseStatus.ErrorCode != null)
                {
                    throw new Exception(respuesta.ResponseStatus.Message);
                }
                
                resultado = respuesta.Tiendas;
            }
            
            return resultado;
            */
            
            string[] resultado = new string[] { };

            var peticionWeb = (HttpWebRequest)HttpWebRequest.Create(this.uriServidorJsonSync + "ListarTiendas");
            peticionWeb.Method = "GET";
            peticionWeb.ContentType = "application/json";
            this.AgregarCookies(peticionWeb);
            
            var respuesta = this.ObtenerRespuesta<ListarTiendasResponse>(peticionWeb);
            if (respuesta.ResponseStatus.ErrorCode != null)
            {
                throw new Exception(respuesta.ResponseStatus.Message);
            }

            return respuesta.Tiendas;
        }

        public string[] ListarBasesDeDatos()
        {
            /*
            this.Conectar();
            string[] resultado = (string[])this.proxy.InvocarMetodo("ListarBasesDeDatos", null);
            this.Desconectar();

            return resultado;
            */
            /*
            string[] resultado = new string[] { };

            using (cliente = new JsonServiceClient(this.uriServidorJsonSync + "ListarBasesDeDatos"))
            {
                var peticion = new ListarBasesDeDatos();
                var respuesta = cliente.Send<ListarBasesDeDatosResponse>(peticion);

                if (respuesta.ResponseStatus.ErrorCode != null)
                {
                    throw new Exception(respuesta.ResponseStatus.Message);
                }

                resultado = respuesta.BasesDeDatos;
            }

            return resultado;*/

            string[] resultado = new string[] { };
            var peticionWeb = (HttpWebRequest)HttpWebRequest.Create(this.uriServidorJsonSync + "ListarBasesDeDatos");
            peticionWeb.Method = "GET";
            peticionWeb.ContentType = "application/json";
            this.AgregarCookies(peticionWeb);
            
            var respuesta = this.ObtenerRespuesta<ListarBasesDeDatosResponse>(peticionWeb);
            if (respuesta.ResponseStatus.ErrorCode != null)
            {
                throw new Exception(respuesta.ResponseStatus.Message);
            }

            return respuesta.BasesDeDatos;
        }

        public string[] ListarTablas(string baseDeDatos)
        {
            /*
            string[] resultado = new string[] { };

            using (cliente = new JsonServiceClient(this.uriServidorJsonSync + "ListarTablas"))
            {
                var peticion = new ListarTablas() { BaseDeDatos = baseDeDatos };
                var respuesta = cliente.Send<ListarTablasResponse>(peticion);

                if (respuesta.ResponseStatus.ErrorCode != null)
                {
                    throw new Exception(respuesta.ResponseStatus.Message);
                }

                resultado = respuesta.Tablas;
            }

            return resultado;
             */

            string[] resultado = new string[] { };

            var peticion = new ListarTablas() { BaseDeDatos = baseDeDatos };
            var peticionWeb = (HttpWebRequest)HttpWebRequest.Create(this.uriServidorJsonSync + "ListarTablas");
            peticionWeb.Method = "POST";
            peticionWeb.ContentType = "application/json";
            this.AgregarCookies(peticionWeb);
            JsonDataContractSerializer.Instance.SerializeToStream<ListarTablas>(peticion, peticionWeb.GetRequestStream());
            
            var respuesta = this.ObtenerRespuesta<ListarTablasResponse>(peticionWeb);
            if (respuesta.ResponseStatus.ErrorCode != null)
            {
                throw new Exception(respuesta.ResponseStatus.Message);
            }

            return respuesta.Tablas;
        }

        public DataTable LeerTabla(int tiendaId, string baseDeDatos, string nombreTabla)
        {
            /*
            DataTable tabla = new DataTable(nombreTabla);

            using (cliente = new JsonServiceClient(this.uriServidorJsonSync + "LeerTabla"))
            {
                var peticion = new LeerTabla() { 
                    TiendaId = tiendaId,
                    BaseDeDatos = baseDeDatos,
                    Tabla = nombreTabla
                };
                var respuesta = cliente.Send<LeerTablaResponse>(peticion);

                if (respuesta.ResponseStatus.ErrorCode != null)
                {
                    throw new Exception(respuesta.ResponseStatus.Message);
                }

                tabla.Dispose();
                tabla = respuesta.Tabla.XmlADataTable();
            }

            return tabla;
             */
            var peticion = new LeerTabla()
            {
                TiendaId = tiendaId,
                BaseDeDatos = baseDeDatos,
                Tabla = nombreTabla
            };
            var peticionWeb = (HttpWebRequest)HttpWebRequest.Create(this.uriServidorJsonSync + "LeerTabla");
            peticionWeb.Method = "POST";
            peticionWeb.ContentType = "application/json";            
            this.AgregarCookies(peticionWeb);
            JsonDataContractSerializer.Instance.SerializeToStream<LeerTabla>(peticion, peticionWeb.GetRequestStream()); 
            
            var respuesta = this.ObtenerRespuesta<LeerTablaResponse>(peticionWeb);
            if (respuesta.ResponseStatus.ErrorCode != null)
            {
                throw new Exception(respuesta.ResponseStatus.Message);
            }

            return respuesta.Tabla.XmlADataTable();
        }

        public bool EscribirTabla(int tiendaId, string baseDeDatos, string nombreTabla, DataTable tabla)
        {
            /*
            bool resultado = false;

            using (cliente = new JsonServiceClient(this.uriServidorJsonSync + "EscribirTabla"))
            {
                var peticion = new EscribirTabla()
                {
                    TiendaId = tiendaId,
                    TablaXml = tabla.DataTableAXml(baseDeDatos, nombreTabla)
                };
                var respuesta = cliente.Send<EscribirTablaResponse>(peticion);

                if (respuesta.ResponseStatus.ErrorCode != null)
                {
                    throw new Exception(respuesta.ResponseStatus.Message);
                }

                resultado = respuesta.Exito;
            }

            return resultado;
             */
            var peticion = new EscribirTabla()
            {
                TiendaId = tiendaId,
                TablaXml = tabla.DataTableAXml(baseDeDatos, nombreTabla)
            };
            var peticionWeb = (HttpWebRequest)HttpWebRequest.Create(this.uriServidorJsonSync + "EscribirTabla");
            peticionWeb.Method = "POST";
            peticionWeb.ContentType = "application/json";
            this.AgregarCookies(peticionWeb);
            JsonDataContractSerializer.Instance.SerializeToStream<EscribirTabla>(peticion, peticionWeb.GetRequestStream());

            var respuesta = this.ObtenerRespuesta<EscribirTablaResponse>(peticionWeb);
            if (respuesta.ResponseStatus.ErrorCode != null)
            {
                throw new Exception(respuesta.ResponseStatus.Message);
            }

            return respuesta.Exito;
        }

        #endregion

        #region Métodos asincrónicos

        public void ListarTiendasAsinc()
        {
            this.CrearTareaAsincronica<EventoListarTiendasCompletadoArgs>(
                this.aleatorio.Next(),
                new Func<string[]>(this.ListarTiendas),
                new SendOrPostCallback(AntesDeDispararListarTiendasCompletado),
                null
            );
        }

        public void ListarBasesDeDatosAsinc()
        {
            this.CrearTareaAsincronica<EventoListarBDsCompletadoArgs>(
                this.aleatorio.Next(),
                new Func<string[]>(this.ListarBasesDeDatos),
                new SendOrPostCallback(AntesDeDispararListarBDsCompletado),
                null
            );
        }

        public void ListarTablasAsinc(string baseDeDatos)
        {
            this.CrearTareaAsincronica<EventoListarTablasCompletadoArgs>(
                this.aleatorio.Next(),
                new Func<string, string[]>(this.ListarTablas),
                new SendOrPostCallback(AntesDeDispararListarTablasCompletado),
                baseDeDatos
            );
        }

        public void LeerTablaAsinc(int tiendaId, string baseDeDatos, string tabla)
        {
            this.CrearTareaAsincronica<EventoLeerTablaCompletadoArgs>(
                this.aleatorio.Next(),
                new Func<int, string, string, DataTable>(this.LeerTabla),
                new SendOrPostCallback(AntesDeDispararLeerTablaCompletado),
                tiendaId, baseDeDatos, tabla
            ); 
        }

        public void EscribirTablaAsinc(int tiendaId, string baseDeDatos, string nombreTabla, DataTable tabla)
        {
            this.CrearTareaAsincronica<EventoEscribirTablaCompletadoArgs>(
                this.aleatorio.Next(),
                new Func<int, string, string, DataTable, bool>(this.EscribirTabla),
                new SendOrPostCallback(AntesDeDispararEscribirTablaCompletado),
                tiendaId, baseDeDatos, nombreTabla, tabla
            ); 
        }

        #endregion

        #endregion
    }
}