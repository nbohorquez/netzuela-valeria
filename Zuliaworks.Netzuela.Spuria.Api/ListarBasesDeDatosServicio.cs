namespace Zuliaworks.Netzuela.Spuria.Api
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.Serialization;
	
	using ServiceStack.ServiceInterface;
	using ServiceStack.ServiceInterface.ServiceModel;		// ResponseStatus
	using Zuliaworks.Netzuela.Spuria.Tipos;
	using Zuliaworks.Netzuela.Valeria.Logica;
	
	public class ListarBasesDeDatosServicio : ServiceBase<ListarBasesDeDatos>
	{
		#region Funciones
		
		protected override object Run (ListarBasesDeDatos request)
		{
			Sesion.Usuario = int.Parse(this.GetSession().FirstName);
			List<string> resultado = new List<string>();

            try
            {
                using (Conexion conexion = new Conexion(Sesion.CadenaDeConexion))
                {
                    conexion.Conectar(Sesion.Credenciales[0], Sesion.Credenciales[1]);
                    string[] basesDeDatos = conexion.ListarBasesDeDatos();

                    var basesDeDatosAMostrar = (from bd in basesDeDatos
                                                where Permisos.EntidadesPermitidas.Keys.Any(k => string.Equals(k, bd, StringComparison.OrdinalIgnoreCase))
                                                select bd).ToList();

                    resultado = basesDeDatosAMostrar;
                }
            }
            catch (Exception ex)
            {
            	//log.Fatal("Usuario: " + this.Cliente + ". Error de listado de base de datos: " + ex.Message);
                throw new Exception("Error de listado de base de datos", ex);
            }
			
            return new ListarBasesDeDatosResponse { BasesDeDatos = resultado.ToArray(), ResponseStatus = new ResponseStatus() };
		}
		
		#endregion
	}
}