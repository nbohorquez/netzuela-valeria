using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;                // ProgressChangedEventArgs, AsyncOperation
using System.Data;                          // DataTable
using System.IO;                            // MemoryStream
using System.Threading;                     // Thread, SendOrPostCallback
using System.Security;                      // SecureString
using Zuliaworks.Netzuela.Spuria.Api;       // DataSetXml

namespace Zuliaworks.Netzuela.Valeria.Datos.Web
{
    public partial class ClienteSpuria
    {
        #region Variables

        private delegate void DelegadoComenzarOperacion(AsyncOperation Asincronico, string Operacion, object[] Parametros);
        private SendOrPostCallback _DelegadoDispararCambioEnProgresoDeOperacion;
        private SendOrPostCallback _DelegadoDispararListarBDsCompletado;
        private SendOrPostCallback _DelegadoDispararListarTablasCompletado;
        private SendOrPostCallback _DelegadoDispararLeerTablaCompletado;
        private SendOrPostCallback _DelegadoDispararEscribirTablaCompletado;
        private SendOrPostCallback _DelegadoDispararCrearUsuarioCompletado;
        private SendOrPostCallback _DelegadoDispararConsultarCompletado;
        private DelegadoComenzarOperacion _Carpintero;

        #endregion

        #region Eventos

        public event EventHandler<ProgressChangedEventArgs> CambioEnProgresoDeOperacion;
        public event EventHandler<EventoOperacionAsincCompletadaArgs> ListarBasesDeDatosCompletado;
        public event EventHandler<EventoOperacionAsincCompletadaArgs> ListarTablasCompletado;
        public event EventHandler<EventoOperacionAsincCompletadaArgs> LeerTablaCompletado;
        public event EventHandler<EventoOperacionAsincCompletadaArgs> EscribirTablaCompletado;
        public event EventHandler<EventoOperacionAsincCompletadaArgs> CrearUsuarioCompletado;
        public event EventHandler<EventoOperacionAsincCompletadaArgs> ConsultarCompletado;

        #endregion

        #region Funciones

        #region Operaciones internas

        private AsyncOperation CrearOperacionAsincronica(object TareaID)
        {
            AsyncOperation Asincronico = null;

            lock (_Hilos.SyncRoot)
            {
                if (_Hilos.Contains(TareaID))
                {
                    throw new ArgumentException("El argumento TareaID debe ser unico", "TareaID");
                }
                else
                {
                    Asincronico = AsyncOperationManager.CreateOperation(TareaID);
                    _Hilos[TareaID] = Asincronico;
                }
            }

            return Asincronico;
        }

        private void ComenzarOperacion(AsyncOperation Asincronico, string Operacion, object[] Parametros)
        {
            List<object> Resultados = new List<object>();
            Exception e = null;

            if (!TareaCancelada(Asincronico.UserSuppliedState))
            {
                try
                {
                    switch (Operacion)
                    {
                        case "ListarBasesDeDatos":
                            Resultados.Add(ListarBasesDeDatos());
                            break;
                        case "ListarTablas":
                            Resultados.Add(ListarTablas((string)Parametros[0]));
                            break;
                        case "LeerTabla":
                            Resultados.Add(LeerTabla((string)Parametros[0], (string)Parametros[1]));
                            break;
                        case "EscribirTabla":
                            Resultados.Add(EscribirTabla(
                                (string)Parametros[0],
                                (string)Parametros[1],
                                (DataTable)Parametros[2]));
                            break;
                        case "CrearUsuario":
                            Resultados.Add(CrearUsuario(
                                (SecureString)Parametros[0],
                                (SecureString)Parametros[1],
                                (string[])Parametros[2],
                                Convert.ToInt16(Parametros[3])));
                            break;
                        case "Consultar":
                            Resultados.Add(Consultar(
                                (string)Parametros[0],
                                (string)Parametros[1]));
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

            this.FinalizarOperacion(Operacion, Resultados.ToArray(), e, TareaCancelada(Asincronico.UserSuppliedState), Asincronico);
        }

        private void FinalizarOperacion(string Operacion, object[] Resultados, Exception Error, bool Cancelado, AsyncOperation Asincronico)
        {
            if (!Cancelado)
            {
                lock (_Hilos.SyncRoot)
                {
                    _Hilos.Remove(Asincronico.UserSuppliedState);
                }
            }

            EventoOperacionAsincCompletadaArgs e =
                        new EventoOperacionAsincCompletadaArgs(Resultados, Cancelado, Error, Asincronico.UserSuppliedState);

            switch (Operacion)
            {
                case "ListarBasesDeDatos":
                    Asincronico.PostOperationCompleted(_DelegadoDispararListarBDsCompletado, e);
                    break;
                case "ListarTablas":
                    Asincronico.PostOperationCompleted(_DelegadoDispararListarTablasCompletado, e);
                    break;
                case "LeerTabla":
                    Asincronico.PostOperationCompleted(_DelegadoDispararLeerTablaCompletado, e);
                    break;
                case "EscribirTabla":
                    Asincronico.PostOperationCompleted(_DelegadoDispararEscribirTablaCompletado, e);
                    break;
                case "CrearUsuario":
                    Asincronico.PostOperationCompleted(_DelegadoDispararCrearUsuarioCompletado, e);
                    break;
                case "Consultar":
                    Asincronico.PostOperationCompleted(_DelegadoDispararConsultarCompletado, e);
                    break;
                default:
                    break;
            }
        }

        private void AntesDeDispararCambioEnProgresoDeOperacion(object Estado)
        {
            ProgressChangedEventArgs e = (ProgressChangedEventArgs)Estado;
            DispararCambioEnProgresoDeOperacion(e);
        }

        protected virtual void DispararCambioEnProgresoDeOperacion(ProgressChangedEventArgs e)
        {
            if (CambioEnProgresoDeOperacion != null)
            {
                CambioEnProgresoDeOperacion(this, e);
            }
        }

        private void AntesDeDispararListarBDsCompletado(object EstadoOperacion)
        {
            EventoOperacionAsincCompletadaArgs e = (EventoOperacionAsincCompletadaArgs)EstadoOperacion;
            DispararListarBDsCompletado(e);
        }

        protected virtual void DispararListarBDsCompletado(EventoOperacionAsincCompletadaArgs e)
        {
            if (ListarBasesDeDatosCompletado != null)
            {
                ListarBasesDeDatosCompletado(this, e);
            }
        }

        private void AntesDeDispararListarTablasCompletado(object EstadoOperacion)
        {
            EventoOperacionAsincCompletadaArgs e = (EventoOperacionAsincCompletadaArgs)EstadoOperacion;
            DispararListarTablasCompletado(e);
        }

        protected virtual void DispararListarTablasCompletado(EventoOperacionAsincCompletadaArgs e)
        {
            if (ListarTablasCompletado != null)
            {
                ListarTablasCompletado(this, e);
            }
        }

        private void AntesDeDispararLeerTablaCompletado(object EstadoOperacion)
        {
            EventoOperacionAsincCompletadaArgs e = (EventoOperacionAsincCompletadaArgs)EstadoOperacion;
            DispararLeerTablaCompletado(e);
        }

        protected virtual void DispararLeerTablaCompletado(EventoOperacionAsincCompletadaArgs e)
        {
            if (LeerTablaCompletado != null)
            {
                LeerTablaCompletado(this, e);
            }
        }

        private void AntesDeDispararEscribirTablaCompletado(object EstadoOperacion)
        {
            EventoOperacionAsincCompletadaArgs e = (EventoOperacionAsincCompletadaArgs)EstadoOperacion;
            DispararEscribirTablaCompletado(e);
        }

        protected virtual void DispararEscribirTablaCompletado(EventoOperacionAsincCompletadaArgs e)
        {
            if (EscribirTablaCompletado != null)
            {
                EscribirTablaCompletado(this, e);
            }
        }

        private void AntesDeDispararCrearUsuarioCompletado(object EstadoOperacion)
        {
            EventoOperacionAsincCompletadaArgs e = (EventoOperacionAsincCompletadaArgs)EstadoOperacion;
            DispararCrearUsuarioCompletado(e);
        }

        protected virtual void DispararCrearUsuarioCompletado(EventoOperacionAsincCompletadaArgs e)
        {
            if (CrearUsuarioCompletado != null)
            {
                CrearUsuarioCompletado(this, e);
            }
        }

        private void AntesDeDispararConsultarCompletado(object EstadoOperacion)
        {
            EventoOperacionAsincCompletadaArgs e = (EventoOperacionAsincCompletadaArgs)EstadoOperacion;
            DispararConsultarCompletado(e);
        }

        protected virtual void DispararConsultarCompletado(EventoOperacionAsincCompletadaArgs e)
        {
            if (ConsultarCompletado != null)
            {
                ConsultarCompletado(this, e);
            }
        }

        #endregion

        #region Métodos sincrónicos

        public string[] ListarBasesDeDatos()
        {
            Conectar();
            string[] Resultado = (string[])_Proxy.InvocarMetodo("ListarBasesDeDatos", null);
            Desconectar();

            return Resultado;
        }

        public string[] ListarTablas(string BaseDeDatos)
        {
            Conectar();
            string[] Resultado = (string[])_Proxy.InvocarMetodo("ListarTablas", BaseDeDatos);
            Desconectar();

            return Resultado;
        }

        public DataTable LeerTabla(string BaseDeDatos, string NombreTabla)
        {
            Conectar();

            object TablaXML = _Proxy.InvocarMetodo("LeerTabla", BaseDeDatos, NombreTabla);
            DataTable Tabla = ((DataTableXml)TablaXML).XmlADataTable();

            Desconectar();

            return Tabla;
        }

        public bool EscribirTabla(string BaseDeDatos, string NombreTabla, DataTable Tabla)
        {
            Conectar();

            DataTableXml DatosAEnviar = Tabla.DataTableAXml(BaseDeDatos, NombreTabla);
            bool Resultado = Convert.ToBoolean(_Proxy.InvocarMetodo("EscribirTabla", DatosAEnviar));
            
            Desconectar();

            return Resultado;
        }

        public bool CrearUsuario(SecureString Usuario, SecureString Contrasena, string[] Columnas, int Privilegios)
        {
            throw new NotImplementedException();
        }

        public DataTable Consultar(string baseDeDatos, string Sql)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Métodos asincrónicos

        public void ListarBasesDeDatosAsinc()
        {
            ListarBasesDeDatosAsinc(_Aleatorio.Next());
        }

        public void ListarTablasAsinc(string BaseDeDatos)
        {
            ListarTablasAsinc(BaseDeDatos, _Aleatorio.Next());
        }

        public void LeerTablaAsinc(string BaseDeDatos, string Tabla)
        {
            LeerTablaAsinc(BaseDeDatos, Tabla, _Aleatorio.Next());
        }

        public void EscribirTablaAsinc(string BaseDeDatos, string NombreTabla, DataTable Tabla)
        {
            EscribirTablaAsinc(BaseDeDatos, NombreTabla, Tabla, _Aleatorio.Next());
        }

        public void CrearUsuarioAsinc(SecureString Usuario, SecureString Contrasena, string[] Columnas, int Privilegios)
        {
            CrearUsuarioAsinc(Usuario, Contrasena, Columnas, Privilegios, _Aleatorio.Next());
        }

        public void ConsultarAsinc(string baseDeDatos, string Sql)
        {
            ConsultarAsinc(baseDeDatos, Sql, _Aleatorio.Next());
        }

        public void ListarBasesDeDatosAsinc(object TareaID)
        {
            try
            {
                AsyncOperation Asincronico = CrearOperacionAsincronica(TareaID);
                _Carpintero.BeginInvoke(Asincronico, "ListarBasesDeDatos", null, null, null);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al iniciar la operacion asincrónica \"" + "ListarBasesDeDatosAsinc\"", ex);
            }
        }

        public void ListarTablasAsinc(string BaseDeDatos, object TareaID)
        {
            try
            {
                object[] Parametros = new object[1] { BaseDeDatos };

                AsyncOperation Asincronico = CrearOperacionAsincronica(TareaID);
                _Carpintero.BeginInvoke(Asincronico, "ListarTablas", Parametros, null, null);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al iniciar la operacion asincrónica \"" + "ListarTablasAsinc\"", ex);
            }
        }

        public void LeerTablaAsinc(string BaseDeDatos, string Tabla, object TareaID)
        {
            try
            {
                object[] Parametros = new object[2] { BaseDeDatos, Tabla };

                AsyncOperation Asincronico = CrearOperacionAsincronica(TareaID);
                _Carpintero.BeginInvoke(Asincronico, "LeerTabla", Parametros, null, null);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al iniciar la operacion asincrónica \"" + "LeerTablaAsinc\"", ex);
            }
        }

        public void EscribirTablaAsinc(string BaseDeDatos, string NombreTabla, DataTable Tabla, object TareaID)
        {
            try
            {
                object[] Parametros = new object[3] { BaseDeDatos, NombreTabla, Tabla };

                AsyncOperation Asincronico = CrearOperacionAsincronica(TareaID);
                _Carpintero.BeginInvoke(Asincronico, "EscribirTabla", Parametros, null, null);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al iniciar la operacion asincrónica \"" + "EscribirTablaAsinc\"", ex);
            }
        }

        public void CrearUsuarioAsinc(SecureString Usuario, SecureString Contrasena, string[] Columnas, int Privilegios, object TareaID)
        {
            try
            {
                object[] Parametros = new object[4] { Usuario, Contrasena, Columnas, Privilegios };

                AsyncOperation Asincronico = CrearOperacionAsincronica(TareaID);
                _Carpintero.BeginInvoke(Asincronico, "CrearUsuario", Parametros, null, null);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al iniciar la operacion asincrónica \"" + "CrearUsuarioAsinc\"", ex);
            }
        }

        public void ConsultarAsinc(string baseDeDatos, string Sql, object TareaID)
        {
            try
            {
                object[] Parametros = new object[2] { baseDeDatos, Sql };

                AsyncOperation Asincronico = CrearOperacionAsincronica(TareaID);
                _Carpintero.BeginInvoke(Asincronico, "Consultar", Parametros, null, null);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al iniciar la operacion asincrónica \"" + "ConsultarAsinc\"", ex);
            }
        }

        #endregion

        #endregion
    }
}

