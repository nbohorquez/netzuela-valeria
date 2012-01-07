using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Security;                              // SecureString
using Zuliaworks.Netzuela.Valeria.Comunes;          // ParametrosDeConexion

namespace Zuliaworks.Netzuela.Valeria.Datos
{
    /// <summary>
    /// Implementa las funciones de acceso a las bases de datos Oracle
    /// </summary>
    public class Oracle : IBaseDeDatos
    {
        #region Variables

        private ConnectionState _Estado;

        #endregion

        #region Constructores

        public Oracle(ParametrosDeConexion ServidorBD) 
        {
            DatosDeConexion = ServidorBD;
        }

        #endregion

        #region Funciones

        public static ServidorLocal DetectarServidor()
        {
            /*
             * Oracle:
             * ======
             * 
             * Por hacer...
             */

            List<ServidorLocal.Instancia> Instancias = new List<ServidorLocal.Instancia>();

            ServidorLocal Serv = new ServidorLocal();
            Serv.Nombre = Constantes.SGBDR.ORACLE;
            Serv.Instancias = Instancias;
            return Serv;
        }

        #endregion

        #region Implementaciones de interfaces

        #region Propiedades

        public ConnectionState Estado
        {
            get { return ConnectionState.Closed; }
        }

        public ParametrosDeConexion DatosDeConexion { get; set; }

        #endregion

        #region Eventos

        public event StateChangeEventHandler CambioDeEstado;
        public event EventHandler<EventoOperacionAsincCompletadaArgs> ListarBasesDeDatosCompletado;
        public event EventHandler<EventoOperacionAsincCompletadaArgs> ListarTablasCompletado;
        public event EventHandler<EventoOperacionAsincCompletadaArgs> LeerTablaCompletado;
        public event EventHandler<EventoOperacionAsincCompletadaArgs> EscribirTablaCompletado;
        public event EventHandler<EventoOperacionAsincCompletadaArgs> CrearUsuarioCompletado;

        #endregion

        #region Funciones

        #region Métodos de eventos

        private void ManejarCambioDeEstado(object Remitente, StateChangeEventArgs Args)
        {
            DispararCambioDeEstado(Args);
        }

        private void ManejarListarBasesDeDatosCompletado(object Remitente, EventoOperacionAsincCompletadaArgs Args)
        {
            DispararListarBasesDeDatosCompletado(Args);
        }

        private void ManejarListarTablasCompletado(object Remitente, EventoOperacionAsincCompletadaArgs Args)
        {
            DispararListarTablasCompletado(Args);
        }

        private void ManejarLeerTablaCompletado(object Remitente, EventoOperacionAsincCompletadaArgs Args)
        {
            DispararLeerTablaCompletado(Args);
        }

        private void ManejarEscribirTablaCompletado(object Remitente, EventoOperacionAsincCompletadaArgs Args)
        {
            DispararEscribirTablaCompletado(Args);
        }

        private void ManejarCrearUsuarioCompletado(object Remitente, EventoOperacionAsincCompletadaArgs Args)
        {
            DispararCrearUsuarioCompletado(Args);
        }

        protected virtual void DispararCambioDeEstado(StateChangeEventArgs e)
        {
            if (CambioDeEstado != null)
            {
                CambioDeEstado(this, e);
            }
        }

        protected virtual void DispararListarBasesDeDatosCompletado(EventoOperacionAsincCompletadaArgs e)
        {
            if (ListarBasesDeDatosCompletado != null)
            {
                ListarBasesDeDatosCompletado(this, e);
            }
        }

        protected virtual void DispararListarTablasCompletado(EventoOperacionAsincCompletadaArgs e)
        {
            if (ListarTablasCompletado != null)
            {
                ListarTablasCompletado(this, e);
            }
        }

        protected virtual void DispararLeerTablaCompletado(EventoOperacionAsincCompletadaArgs e)
        {
            if (LeerTablaCompletado != null)
            {
                LeerTablaCompletado(this, e);
            }
        }

        protected virtual void DispararEscribirTablaCompletado(EventoOperacionAsincCompletadaArgs e)
        {
            if (EscribirTablaCompletado != null)
            {
                EscribirTablaCompletado(this, e);
            }
        }

        protected virtual void DispararCrearUsuarioCompletado(EventoOperacionAsincCompletadaArgs e)
        {
            if (CrearUsuarioCompletado != null)
            {
                CrearUsuarioCompletado(this, e);
            }
        }

        #endregion

        #region Métodos sincrónicos

        public void Conectar(SecureString Usuario, SecureString Contrasena) { }

        public void Desconectar() { }

        public string[] ListarBasesDeDatos() 
        { 
            string[] Resultado = new string[] { };
            return Resultado;
        }

        public string[] ListarTablas(string BaseDeDatos)
        {
            string[] Resultado = new string[] { };
            return Resultado;
        }

        public DataTable LeerTabla(string BaseDeDatos, string Tabla)
        {
            return new DataTable();
        }

        public bool EscribirTabla(string BaseDeDatos, string NombreTabla, DataTable Tabla)
        {
            throw new NotImplementedException();
        }

        public object CrearUsuario(SecureString Usuario, SecureString Contrasena, string[] Columnas, int Privilegios)
        {
            throw new NotImplementedException();
        }

        #endregion 

        #region Métodos asincrónicos

        public void ListarBasesDeDatosAsinc()
        {
            throw new NotImplementedException();
        }

        public void ListarTablasAsinc(string BaseDeDatos)
        {
            throw new NotImplementedException();
        }

        public void LeerTablaAsinc(string BaseDeDatos, string Tabla)
        {
            throw new NotImplementedException();
        }

        public void EscribirTablaAsinc(string BaseDeDatos, string NombreTabla, DataTable Tabla)
        {
            throw new NotImplementedException();
        }

        public void CrearUsuarioAsinc(SecureString Usuario, SecureString Contrasena, string[] Columnas, int Privilegios)
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion

        #endregion
    }
}
