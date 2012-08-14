namespace Zuliaworks.Netzuela.Spuria.Api
{
	using System;
	using System.Net;
	
	using ServiceStack.CacheAccess;							// ICacheClient
	using ServiceStack.CacheAccess.Providers;				// MemoryCacheClient
	using ServiceStack.ServiceInterface;					// AuthFeature
	using ServiceStack.ServiceInterface.Auth;				// IAuthProvider
	using ServiceStack.WebHost.Endpoints;					// AppHostBase
	
	public class Anfitrion : AppHostBase
	{
		#region Constructores
		
		public Anfitrion()
			: base("API de Netzuela", typeof(ListarTiendasServicio).Assembly) 
		{
		}
		
		#endregion
		
		#region Funciones

		public override void Configure(Funq.Container container) 
		{ 
			this.Plugins.Add(new AuthFeature(() => new AuthUserSession(), new IAuthProvider[] {
            	new Autentificacion()
        	}));/*
			this.RequestFilters.Add((pet, resp, dto) =>
			{ 
				if (!pet.IsSecureConnection)
				{
					((HttpWebResponse)resp).StatusCode = (int)HttpStatusCode.Forbidden;
					((HttpWebResponse)resp).Close();
				}
			});*/
		    container.Register<ICacheClient>(new MemoryCacheClient());
		}

		#endregion
	}
}