namespace Zuliaworks.Netzuela.Valeria.Datos.Web
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;                        // ProgressChangedEventArgs, AsyncOperation
    using System.Data;                                  // DataTable
    using System.IO;                                    // MemoryStream
    using System.Net;
    using System.Security;                              // SecureString
    using System.Threading;                             // Thread, SendOrPostCallback
    using System.Web;

    using ServiceStack.ServiceClient.Web;               // JsonServiceClient
    using Zuliaworks.Netzuela.Valeria.Datos.Eventos;
    using Zuliaworks.Netzuela.Spuria.Tipos;
    using ServiceStack.ServiceModel.Serialization;             // DataSetXml
    

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
        private DelegadoComenzarOperacion carpintero;
        private delegate void DelegadoComenzarOperacion(AsyncOperation asincronico, string operacion, object[] parametros);

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

        private AsyncOperation CrearOperacionAsincronica(object tareaId)
        {
            AsyncOperation asincronico = null;

            lock (this.hilos.SyncRoot)
            {
                if (this.hilos.Contains(tareaId))
                {
                    throw new ArgumentException("El argumento tareaId debe ser unico", "tareaId");
                }
                else
                {
                    asincronico = AsyncOperationManager.CreateOperation(tareaId);
                    this.hilos[tareaId] = asincronico;
                }
            }

            return asincronico;
        }

        private void ComenzarOperacion(AsyncOperation asincronico, string operacion, object[] parametros)
        {
            List<object> resultados = new List<object>();
            Exception e = null;

            if (!TareaCancelada(asincronico.UserSuppliedState))
            {
                try
                {
                    switch (operacion)
                    {
                        case "ListarTiendas":
                            resultados.Add(this.ListarTiendas());
                            break;
                        case "ListarBasesDeDatos":
                            resultados.Add(this.ListarBasesDeDatos());
                            break;
                        case "ListarTablas":
                            resultados.Add(this.ListarTablas((string)parametros[0]));
                            break;
                        case "LeerTabla":
                            resultados.Add(this.LeerTabla((int)parametros[0], (string)parametros[1], (string)parametros[2]));
                            break;
                        case "EscribirTabla":
                            resultados.Add(this.EscribirTabla(
                                (int)parametros[0],
                                (string)parametros[1],
                                (string)parametros[2],
                                (DataTable)parametros[3]));
                            break;
                        case "CrearUsuario":
                            resultados.Add(this.CrearUsuario(
                                (SecureString)parametros[0],
                                (SecureString)parametros[1],
                                (string[])parametros[2],
                                Convert.ToInt16(parametros[3])));
                            break;
                        case "Consultar":
                            resultados.Add(this.Consultar(
                                (string)parametros[0],
                                (string)parametros[1]));
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    e = ex;
                }
            }

            this.FinalizarOperacion(operacion, resultados.ToArray(), e, TareaCancelada(asincronico.UserSuppliedState), asincronico);
        }

        private void FinalizarOperacion(string operacion, object[] resultados, Exception error, bool cancelado, AsyncOperation asincronico)
        {
            if (!cancelado)
            {
                lock (this.hilos.SyncRoot)
                {
                    this.hilos.Remove(asincronico.UserSuppliedState);
                }
            }

            switch (operacion)
            {
                case "ListarTiendas":
                    {
                        EventoListarTiendasCompletadoArgs e =
                            new EventoListarTiendasCompletadoArgs(resultados, cancelado, error, asincronico.UserSuppliedState);
                        asincronico.PostOperationCompleted(this.delegadoDispararListarTiendasCompletado, e);
                        break;
                    }

                case "ListarBasesDeDatos":
                    {
                        EventoListarBDsCompletadoArgs e =
                            new EventoListarBDsCompletadoArgs(resultados, cancelado, error, asincronico.UserSuppliedState);
                        asincronico.PostOperationCompleted(this.delegadoDispararListarBDsCompletado, e);
                        break;
                    }

                case "ListarTablas":
                    {
                        EventoListarTablasCompletadoArgs e =
                            new EventoListarTablasCompletadoArgs(resultados, cancelado, error, asincronico.UserSuppliedState);
                        asincronico.PostOperationCompleted(this.delegadoDispararListarTablasCompletado, e);
                        break;
                    }

                case "LeerTabla":
                    {
                        EventoLeerTablaCompletadoArgs e =
                            new EventoLeerTablaCompletadoArgs(resultados, cancelado, error, asincronico.UserSuppliedState);
                        asincronico.PostOperationCompleted(this.delegadoDispararLeerTablaCompletado, e);
                        break;
                    }

                case "EscribirTabla":
                    {
                        EventoEscribirTablaCompletadoArgs e =
                            new EventoEscribirTablaCompletadoArgs(resultados, cancelado, error, asincronico.UserSuppliedState);
                        asincronico.PostOperationCompleted(this.delegadoDispararEscribirTablaCompletado, e);
                        break;
                    }

                case "CrearUsuario":
                    {
                        EventoCrearUsuarioCompletadoArgs e =
                            new EventoCrearUsuarioCompletadoArgs(resultados, cancelado, error, asincronico.UserSuppliedState);
                        asincronico.PostOperationCompleted(this.delegadoDispararCrearUsuarioCompletado, e);
                        break;
                    }

                case "Consultar":
                    {
                        EventoConsultarCompletadoArgs e =
                            new EventoConsultarCompletadoArgs(resultados, cancelado, error, asincronico.UserSuppliedState);
                        asincronico.PostOperationCompleted(this.delegadoDispararConsultarCompletado, e);
                        break;
                    }

                default:
                    break;
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

        public bool CrearUsuario(SecureString usuario, SecureString contrasena, string[] columnas, int privilegios)
        {
            throw new NotImplementedException();
        }

        public DataTable Consultar(string baseDeDatos, string sql)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Métodos asincrónicos

        public void ListarTiendasAsinc()
        {
            this.ListarTiendasAsinc(this.aleatorio.Next());
        }

        public void ListarBasesDeDatosAsinc()
        {
            this.ListarBasesDeDatosAsinc(this.aleatorio.Next());
        }

        public void ListarTablasAsinc(string baseDeDatos)
        {
            this.ListarTablasAsinc(baseDeDatos, this.aleatorio.Next());
        }

        public void LeerTablaAsinc(int tiendaId, string baseDeDatos, string tabla)
        {
            this.LeerTablaAsinc(tiendaId, baseDeDatos, tabla, this.aleatorio.Next());
        }

        public void EscribirTablaAsinc(int tiendaId, string baseDeDatos, string nombreTabla, DataTable tabla)
        {
            this.EscribirTablaAsinc(tiendaId, baseDeDatos, nombreTabla, tabla, this.aleatorio.Next());
        }

        public void CrearUsuarioAsinc(SecureString usuario, SecureString contrasena, string[] columnas, int privilegios)
        {
            this.CrearUsuarioAsinc(usuario, contrasena, columnas, privilegios, this.aleatorio.Next());
        }

        public void ConsultarAsinc(string baseDeDatos, string Sql)
        {
            this.ConsultarAsinc(baseDeDatos, Sql, this.aleatorio.Next());
        }

        public void ListarTiendasAsinc(object tareaId)
        {
            try
            {
                AsyncOperation asincronico = this.CrearOperacionAsincronica(tareaId);
                this.carpintero.BeginInvoke(asincronico, "ListarTiendas", null, null, null);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al iniciar la operacion asincrónica \"ListarTiendasAsinc\"", ex);
            }
        }

        public void ListarBasesDeDatosAsinc(object tareaId)
        {
            try
            {
                AsyncOperation asincronico = this.CrearOperacionAsincronica(tareaId);
                this.carpintero.BeginInvoke(asincronico, "ListarBasesDeDatos", null, null, null);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al iniciar la operacion asincrónica \"ListarBasesDeDatosAsinc\"", ex);
            }
        }

        public void ListarTablasAsinc(string baseDeDatos, object tareaId)
        {
            try
            {
                object[] parametros = new object[1] { baseDeDatos };

                AsyncOperation asincronico = this.CrearOperacionAsincronica(tareaId);
                this.carpintero.BeginInvoke(asincronico, "ListarTablas", parametros, null, null);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al iniciar la operacion asincrónica \"ListarTablasAsinc\"", ex);
            }
        }

        public void LeerTablaAsinc(int tiendaId, string baseDeDatos, string tabla, object tareaId)
        {
            try
            {
                object[] parametros = new object[3] { tiendaId, baseDeDatos, tabla };

                AsyncOperation asincronico = this.CrearOperacionAsincronica(tareaId);
                this.carpintero.BeginInvoke(asincronico, "LeerTabla", parametros, null, null);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al iniciar la operacion asincrónica \"LeerTablaAsinc\"", ex);
            }
        }

        public void EscribirTablaAsinc(int tiendaId, string baseDeDatos, string nombreTabla, DataTable tabla, object tareaId)
        {
            try
            {
                object[] parametros = new object[4] { tiendaId, baseDeDatos, nombreTabla, tabla };

                AsyncOperation asincronico = this.CrearOperacionAsincronica(tareaId);
                this.carpintero.BeginInvoke(asincronico, "EscribirTabla", parametros, null, null);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al iniciar la operacion asincrónica \"EscribirTablaAsinc\"", ex);
            }
        }

        public void CrearUsuarioAsinc(SecureString usuario, SecureString contrasena, string[] columnas, int privilegios, object tareaId)
        {
            try
            {
                object[] parametros = new object[4] { usuario, contrasena, columnas, privilegios };

                AsyncOperation asincronico = this.CrearOperacionAsincronica(tareaId);
                this.carpintero.BeginInvoke(asincronico, "CrearUsuario", parametros, null, null);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al iniciar la operacion asincrónica \"CrearUsuarioAsinc\"", ex);
            }
        }

        public void ConsultarAsinc(string baseDeDatos, string sql, object tareaId)
        {
            try
            {
                object[] parametros = new object[2] { baseDeDatos, sql };

                AsyncOperation asincronico = this.CrearOperacionAsincronica(tareaId);
                this.carpintero.BeginInvoke(asincronico, "Consultar", parametros, null, null);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al iniciar la operacion asincrónica \"ConsultarAsinc\"", ex);
            }
        }

        #endregion

        #endregion
    }
}