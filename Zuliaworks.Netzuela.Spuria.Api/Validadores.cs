namespace Zuliaworks.Netzuela.Spuria.Api
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
	
	using ServiceStack.FluentValidation.Validators;
	using Zuliaworks.Netzuela.Valeria.Logica;

	public static class Validadores
	{
		#region Constantes
		
		public const string ERROR_TABLA = "La tabla especificada no existe en la base de datos o no esta permitida";
		public const string ERROR_BASE_DE_DATOS = "La base de datos especificada no existe o no esta permitida";
		public const string ERROR_TIENDA_ID = "La tienda especificada no pertenece a este cliente o no existe";
		
		#endregion
		
		#region Funciones
	
		public static bool Tabla(string baseDeDatos, string tabla)
		{
			return Permisos.EntidadesPermitidas[baseDeDatos].Any(e => string.Equals(e.Nombre, tabla, StringComparison.OrdinalIgnoreCase));
		}
		
		public static bool BaseDeDatos(string baseDeDatos)
		{
			return Permisos.EntidadesPermitidas.Keys.Contains(baseDeDatos);
		}
		
		public static bool TiendaId(int tiendaId)
		{
			bool resultado = false;
			
			using (Conexion conexion = new Conexion(Sesion.CadenaDeConexion))
            {
				conexion.Conectar(Sesion.Credenciales[0], Sesion.Credenciales[1]);

                string sql = "SELECT t.tienda_id "
                			+ "FROM tienda AS t "
                			+ "JOIN cliente AS c ON t.cliente_p = c.rif "
							+ "JOIN usuario AS u ON c.propietario = u.usuario_id "
							+ "WHERE u.usuario_id = " + Sesion.Usuario.ToString();
                DataTable t = conexion.Consultar(Constantes.BaseDeDatos, sql);				
				resultado = t.Rows.Cast<DataRow>().Any(r => tiendaId == (int)r.ItemArray[0]);
            }
			
			return resultado;
		}
		
		#endregion
	}
}
