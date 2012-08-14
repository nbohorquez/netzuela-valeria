namespace Zuliaworks.Netzuela.Spuria.Api
{
	using log4net;
	
	using System;
	using System.IdentityModel;
	using System.IdentityModel.Selectors;
	using System.IdentityModel.Tokens;
	using System.ServiceModel;
	using System.ServiceModel.Channels;
	using System.Web;

	public class ValidadorCredenciales : UserNamePasswordValidator
	{
		#region Variables y Constantes
		
		private readonly ILog log;
		
		#endregion
		
		#region Constructores
		
		public ValidadorCredenciales ()
		{
			log = LogManager.GetLogger(typeof(ValidadorCredenciales));
		}
		
		#endregion

		#region Implementacion de interfaces
		
		public override void Validate (string userName, string password)
		{
			// Tengo que dejar este validador aqui para que puedan pasar los mensajes con los credenciales
			// a WcfInspectorMensajes
			log.Debug("UserNamePasswordValidator");
		}
		
		#endregion
	}
}

