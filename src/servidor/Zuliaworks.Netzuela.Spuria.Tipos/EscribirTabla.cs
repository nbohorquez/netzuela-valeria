namespace Zuliaworks.Netzuela.Spuria.Tipos
{
	using System;
	using System.Runtime.Serialization;						// DataContract, DataMember
	
	using ServiceStack.ServiceHost;							// RestService
	using ServiceStack.ServiceInterface;					// Authenticate
	
	[DataContract]
	[Authenticate()]
	[RestService("/escribirtabla")]
	public class EscribirTabla
	{
		[DataMember]
		public int TiendaId { get; set; }
		[DataMember]
		public DataTableXml TablaXml { get; set; }
	}
}

