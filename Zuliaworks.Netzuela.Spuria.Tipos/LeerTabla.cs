namespace Zuliaworks.Netzuela.Spuria.Tipos
{
	using System;
	using System.Runtime.Serialization;						// DataContract, DataMember
	
	using ServiceStack.ServiceHost;							// RestService
	using ServiceStack.ServiceInterface;					// Authenticate
	
	[DataContract]
	[Authenticate()]
	[RestService("/leertabla")]
	public class LeerTabla
	{
		[DataMember]
		public int TiendaId { get; set; }
		[DataMember]
		public string BaseDeDatos { get; set; }
		[DataMember]
		public string Tabla { get; set; }
	}	
}

