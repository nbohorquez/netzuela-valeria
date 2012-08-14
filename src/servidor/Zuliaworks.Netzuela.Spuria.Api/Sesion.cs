namespace Zuliaworks.Netzuela.Spuria.Api
{
	using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security;                              // SecureString
	
	//using log4net;
    using Zuliaworks.Netzuela.Valeria.Comunes;          // ParametrosDeConexion
    using Zuliaworks.Netzuela.Valeria.Preferencias;     // CargarGuardar
	
    public static class Sesion
    {
		//private static readonly ILog log;
		//private static readonly ParametrosDeConexion parametros;
        //private static readonly SecureString[] credenciales;
		private static readonly Dictionary<string,object> propiedades;
		
        static Sesion()
        {
			try
			{
				propiedades = new Dictionary<string, object>() 
                {
					{ "usuario_id", -1 }
                };
				propiedades["parametros"] = CargarGuardar.CargarParametrosDeConexion("Local");
				propiedades["credenciales"] = CargarGuardar.CargarCredenciales("Local");
	            
				if (CadenaDeConexion == null || Credenciales.Length != 2)
	            {
	                throw new Exception("Error interno del servidor. Por favor inténtelo más tarde");
	            }
			}
			catch (Exception ex)
			{
				//log.Fatal("Error al obtener los datos de conexion de la base de datos: " + ex.Message);
				throw new Exception("Error al obtener los datos de conexion de la base de datos", ex);
			}
        }

        public static ParametrosDeConexion CadenaDeConexion
        {
            get { return (ParametrosDeConexion)propiedades["parametros"]; }
		}
		
        public static SecureString[] Credenciales
        {
            get { return (SecureString[])propiedades["credenciales"]; }
		}
		
		public static int Usuario
		{
			get 
			{ 
				return (int)propiedades["usuario_id"]; 
			}
			set
			{
				propiedades["usuario_id"] = value;
			}
		}
    }
}