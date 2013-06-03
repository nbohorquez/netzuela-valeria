namespace Zuliaworks.Netzuela.Valeria.Servidor.Api {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security;                              // SecureString
    
    //using log4net;
    using Zuliaworks.Netzuela.Valeria.Comunes;          // ParametrosDeConexion
    using Zuliaworks.Netzuela.Valeria.Preferencias;     // CargarGuardar
    
    public static class Sesion {
        //private static readonly ILog log;
        private static readonly Dictionary<string,object> propiedades;
        private static readonly object bloqueo;
        
        static Sesion() {
            try {
                bloqueo = new object();
                propiedades = new Dictionary<string, object>();
                propiedades["parametros"] = CargarGuardar.CargarParametrosDeConexion("Local");
                propiedades["credenciales"] = CargarGuardar.CargarCredenciales("Local");
                
                if (CadenaDeConexion == null || Credenciales.Length != 2) {
                    throw new Exception("Error interno del servidor. Por favor inténtelo más tarde");
                }
            } catch (Exception ex) {
                //log.Fatal("Error al obtener los datos de conexion de la base de datos: " + ex.Message);
                throw new Exception("Error al obtener los datos de conexion de la base de datos", ex);
            }
        }

        public static ParametrosDeConexion CadenaDeConexion {
            get { 
                lock(bloqueo) {
                    return (ParametrosDeConexion)propiedades["parametros"]; 
                }
            }
        }
        
        public static SecureString[] Credenciales {
            get { 
                lock(bloqueo) {
                    return (SecureString[])propiedades["credenciales"];
                }
            }
        }
    }
}