namespace Zuliaworks.Netzuela.Spuria.Tipos
{
	using System;
	using System.Runtime.Serialization;						// DataContract, DataMember
	
	using ServiceStack.ServiceInterface.ServiceModel;		// IHasResponseStatus
	
	[DataContract]
	public class ListarBasesDeDatosResponse : IHasResponseStatus
	{
		[DataMember]
		public string[] BasesDeDatos { get; set; }
		[DataMember]
		public ResponseStatus ResponseStatus { get; set; }
	}
}

