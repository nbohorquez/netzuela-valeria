namespace Zuliaworks.Netzuela.Spuria.Tipos
{
	using System;
	using System.Runtime.Serialization;						// DataContract, DataMember
	
	using ServiceStack.ServiceHost;							// RestService
	using ServiceStack.ServiceInterface;					// Authenticate
	
	[DataContract]
	[Authenticate()]
	[RestService("/listartablas")]
	public class ListarTablas
	{
		[DataMember]
		public string BaseDeDatos { get; set; }
	}
}