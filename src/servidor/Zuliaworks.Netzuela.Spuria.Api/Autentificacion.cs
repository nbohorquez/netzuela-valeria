//-----------------------------------------------------------------------
// <copyright file="AutentificacionExtensiones.cs" company="Zuliaworks">
//     Copyright (c) Zuliaworks. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Zuliaworks.Netzuela.Spuria.Api
{
	using System;
    using System.Collections.Generic;
    using System.Data;                              // DataTable
    using System.Linq;
	using System.Net;								// WebHeaderCollection
	using System.ServiceModel.Channels;				// HttpRequestMessageProperty
	using System.Web;								// HttpContext, HttpRequest
	
	using BCrypt.Net;
	using log4net;
	using ServiceStack.ServiceInterface;			// IServiceBase
	using ServiceStack.ServiceInterface.Auth;		// CredentialsAuthProvider
	using Zuliaworks.Netzuela.Valeria.Comunes;
	using Zuliaworks.Netzuela.Valeria.Logica;
		
	/// <summary>
    /// Administra los credenciales enviados por el cliente.
    /// </summary>
	public class Autentificacion : CredentialsAuthProvider
	{
		#region Variables
		
		//private readonly ILog log;
		private string correo_electronico;
		private int usuario_id;
		
		#endregion
		
		#region Constructores
		
		public Autentificacion()
		{
			//log = LogManager.GetLogger(typeof(Autentificacion));
			this.correo_electronico = string.Empty;
			this.usuario_id = -1;
		}
		
		#endregion
		
		#region Funciones
		
	    public override bool TryAuthenticate(IServiceBase authService, string userName, string password)
	    {
			bool resultado = false;
			
	        using (Conexion conexion = new Conexion(Sesion.CadenaDeConexion))
            {
				conexion.Conectar(Sesion.Credenciales[0], Sesion.Credenciales[1]);
				string sql = "SELECT acceso_id, contrasena FROM acceso WHERE correo_electronico = '" + userName + "'";
                DataTable t = conexion.Consultar(Constantes.BaseDeDatos, sql);
				
                if (t.Rows.Count == 1)
                {
					string contrasena = System.Text.Encoding.UTF8.GetString((byte[])t.Rows[0].ItemArray[1]);
					resultado = BCrypt.Verify(password, contrasena);
									
					if (resultado)
					{
						this.correo_electronico = userName;
						this.usuario_id = (int)t.Rows[0].ItemArray[0];
					}
				}
            }
						
			return resultado;
		}
	
	    public override void OnAuthenticated(IServiceBase authService, IAuthSession session, IOAuthTokens tokens, Dictionary<string, string> authInfo)
	    {
			session.Email = this.correo_electronico;
			session.FirstName = this.usuario_id.ToString();
	        authService.SaveSession(session, this.SessionExpiry);
	    }
			
		#endregion
	}
}