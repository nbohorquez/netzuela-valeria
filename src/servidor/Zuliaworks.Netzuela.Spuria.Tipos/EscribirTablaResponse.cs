namespace Zuliaworks.Netzuela.Spuria.Tipos
{
	using System;
	using System.Runtime.Serialization;						// DataContract, DataMember
	
	using ServiceStack.ServiceInterface.ServiceModel;		// IHasResponseStatus
	
	[DataContract]
	public class EscribirTablaResponse : IHasResponseStatus
	{
		[DataMember]
		public bool Exito { get; set; }
		[DataMember]
		public ResponseStatus ResponseStatus { get; set; }
	}
}

