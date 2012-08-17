namespace Zuliaworks.Netzuela.Valeria.Tipos
{
	using System;
	using System.Runtime.Serialization;						// DataContract, DataMember
	
	using ServiceStack.ServiceHost;							// RestService
	using ServiceStack.ServiceInterface;					// Authenticate
	
	[DataContract]
	[Authenticate()]
	[RestService("/listarbasesdedatos")]
	public class ListarBasesDeDatos
	{
	}
}