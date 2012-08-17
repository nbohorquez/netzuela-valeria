namespace Zuliaworks.Netzuela.Valeria.Servidor.Tipos
{
	using System;
	using System.Runtime.Serialization;						// DataContract, DataMember
	
	using ServiceStack.ServiceInterface.ServiceModel;		// IHasResponseStatus
	
	[DataContract]
	public class ListarTiendasResponse : IHasResponseStatus
	{
		[DataMember]
		public string[] Tiendas { get; set; }
		[DataMember]
		public ResponseStatus ResponseStatus { get; set; }
	}
}