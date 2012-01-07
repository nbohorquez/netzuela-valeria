﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;                                  // ConnectionState, DataTable
using System.Security;                              // SecureString
using Zuliaworks.Netzuela.Paris.ContratoValeria;    // DataSetXML
using Zuliaworks.Netzuela.Valeria.Comunes;          // DatosDeConexion
using Zuliaworks.Netzuela.Valeria.Datos.Web;        // ProxyDinamico

namespace Zuliaworks.Netzuela.Valeria.Datos
{
    /// <summary>
    /// Implementa las funciones de acceso a las bases de datos de Netzuela en Internet
    /// </summary>
    public class Netzuela : IBaseDeDatos
    {
        #region Variables

        private ClienteValeria _Cliente;

        // ¡Temporal!
        private ConnectionState _Estado;

        #endregion

        #region Constructores

        public Netzuela(ParametrosDeConexion ServidorBD)
        {
            DatosDeConexion = ServidorBD;

            // El argumento de ClienteValeria debe estar relacionado con DatosDeConexion
            _Cliente = new ClienteValeria();

            // Inicializamos los manejadores de eventos
            _Cliente.ListarBasesDeDatosCompletado += new EventHandler<EventoOperacionAsincCompletadaArgs>(ManejarListarBasesDeDatosCompletado);
            _Cliente.ListarTablasCompletado += new EventHandler<EventoOperacionAsincCompletadaArgs>(ManejarListarTablasCompletado);
            _Cliente.LeerTablaCompletado += new EventHandler<EventoOperacionAsincCompletadaArgs>(ManejarLeerTablaCompletado);
            _Cliente.EscribirTablaCompletado += new EventHandler<EventoOperacionAsincCompletadaArgs>(ManejarEscribirTablaCompletado);
            _Cliente.CrearUsuarioCompletado += new EventHandler<EventoOperacionAsincCompletadaArgs>(ManejarCrearUsuarioCompletado);
            
            // Hay que ver como quito este pedazo de codigo tan feo
            _Estado = ConnectionState.Closed;
        }

        #endregion

        #region Implementaciones de interfaces

        #region Propiedades

        public ConnectionState Estado
        {
            get { return _Estado; }
            private set
            {
                if(value != _Estado)
                {
                    ConnectionState Anterior = _Estado;
                    _Estado = value;
                    DispararCambioDeEstado(new StateChangeEventArgs(Anterior, _Estado));
                }
            }
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

        public void Conectar(SecureString Usuario, SecureString Contrasena)
        {
            try
            {
                if (_Cliente != null)
                {
                    Desconectar();
                }

                _Cliente.UriWsdlServicio = "http://localhost:4757/Servidor.svc?wsdl";
                _Cliente.Conectar();
                
                // Esto hay que borrarlo
                Estado = ConnectionState.Open;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al establecer la conexión con el servidor de Netzuela", ex);
            }
        }

        public void Desconectar()
        {
            try
            {
                if (_Cliente != null)
                {
                    _Cliente.Desconectar();
                }
                
                // Esto hay que borrarlo
                Estado = ConnectionState.Closed;
            }
            catch(Exception ex)
            {
                throw new Exception("Error al cerrar la conexión con el servidor de Netzuela", ex);
            }
        }

        public string[] ListarBasesDeDatos()
        {
            List<string> Resultado = new List<string>();

            try
            {
                Resultado = _Cliente.ListarBasesDeDatos().ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las bases de datos", ex);
            }
            
            return Resultado.ToArray();
        }

        public string[] ListarTablas(string BaseDeDatos)
        {
            List<string> Resultado = new List<string>();

            try
            {
                Resultado = _Cliente.ListarTablas(BaseDeDatos).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las tablas", ex);
            }

            return Resultado.ToArray();
        }

        public DataTable LeerTabla(string BaseDeDatos, string Tabla)
        {            
            DataTable Resultado = new DataTable();

            try
            {
                Resultado = _Cliente.LeerTabla(BaseDeDatos, Tabla);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las tablas", ex);
            }

            return Resultado;
        }

        public bool EscribirTabla(string BaseDeDatos, string NombreTabla, DataTable Tabla)
        {
            bool Resultado = false;

            try
            {
                Resultado = _Cliente.EscribirTabla(BaseDeDatos, NombreTabla, Tabla);
            }
            catch (Exception ex)
            {
                string Error = "Error al escribir la tabla " + NombreTabla + " en la base de datos " + BaseDeDatos;
                throw new Exception(Error, ex);
            }

            return Resultado;
        }

        public object CrearUsuario(SecureString Usuario, SecureString Contrasena, string[] Columnas, int Privilegios)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Métodos asincrónicos

        public void ListarBasesDeDatosAsinc()
        {
            try
            {
                _Cliente.ListarBasesDeDatosAsinc();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las bases de datos", ex);
            }
        }

        public void ListarTablasAsinc(string BaseDeDatos)
        {
            try
            {
                _Cliente.ListarTablasAsinc(BaseDeDatos);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las tablas", ex);
            }
        }

        public void LeerTablaAsinc(string BaseDeDatos, string Tabla)
        {
            try
            {
                _Cliente.LeerTablaAsinc(BaseDeDatos, Tabla);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al leer la tabla", ex);
            }
        }

        public void EscribirTablaAsinc(string BaseDeDatos, string NombreTabla, DataTable Tabla)
        {
            try
            {
                _Cliente.EscribirTablaAsinc(BaseDeDatos, NombreTabla, Tabla);
            }
            catch (Exception ex)
            {
                string Error = "Error al escribir la tabla " + NombreTabla + " en la base de datos " + BaseDeDatos;
                throw new Exception(Error, ex);
            }
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
