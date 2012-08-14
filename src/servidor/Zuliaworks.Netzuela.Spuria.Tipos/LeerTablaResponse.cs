namespace Zuliaworks.Netzuela.Spuria.Tipos
{
	using System;
	using System.Runtime.Serialization;						// DataContract, DataMember
	
	using ServiceStack.ServiceInterface.ServiceModel;		// IHasResponseStatus
	
	[DataContract]
	public class LeerTablaResponse : IHasResponseStatus
	{
		[DataMember]
		public DataTableXml Tabla { get; set; }
		[DataMember]
		public ResponseStatus ResponseStatus { get; set; }
	}
}

