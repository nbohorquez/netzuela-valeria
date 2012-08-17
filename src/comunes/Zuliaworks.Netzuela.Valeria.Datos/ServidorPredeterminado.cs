﻿namespace Zuliaworks.Netzuela.Valeria.Datos
{
    using System;
    using System.Collections.Generic;
    using System.Data;                                  // DataTable, ConnectionState
    using System.Linq;
    using System.Security;                              // SecureString
    using System.Text;

    using Zuliaworks.Netzuela.Valeria.Comunes;          // DatosDeConexion

    /// <summary>
    /// Esta clase se carga cuando no se ha podido conectar con otro proveedor de acceso.
    /// </summary>
    public class ServidorPredeterminado : EventosComunes, IBaseDeDatosLocal
    {
        #region Variables

        private ConnectionState estado;

        #endregion

        #region Constructores

        public ServidorPredeterminado(ParametrosDeConexion servidorBd)
        {
            this.DatosDeConexion = servidorBd;
            this.estado = ConnectionState.Closed;
        }

        ~ServidorPredeterminado()
        {
            this.Dispose(false);
        }

        #endregion

        #region Funciones

        protected void Dispose(bool BorrarCodigoAdministrado)
        {
            this.DatosDeConexion = null;
            
            if (BorrarCodigoAdministrado)
            {
            }
        }

        #endregion

        #region Implementaciones de interfaces

        #region Propiedades

        public ConnectionState Estado
        {
            get { return this.estado; }
        }

        public ParametrosDeConexion DatosDeConexion { get; set; }

        #endregion
        
        #region Funciones
        
        #region Métodos sincrónicos

        public void Conectar(SecureString Usuario, SecureString Contrasena) 
        { 
        }

        public void Desconectar() 
        { 
        }

        public string[] ListarBasesDeDatos()
        {
            return new string[] { };
        }

        public string[] ListarTablas(string BaseDeDatos)
        {
            return new string[] { };
        }

        public DataTable LeerTabla(string BaseDeDatos, string Tabla)
        {
            return new DataTable();
        }

        public bool EscribirTabla(string BaseDeDatos, string NombreTabla, DataTable Tabla)
        {
            throw new NotImplementedException();
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

        public void ConsultarAsinc(string baseDeDatos, string Sql)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            /*
             * En este enlace esta la mejor explicacion acerca de como implementar IDisposable
             * http://stackoverflow.com/questions/538060/proper-use-of-the-idisposable-interface
             */

            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #endregion

        #endregion
    }
}