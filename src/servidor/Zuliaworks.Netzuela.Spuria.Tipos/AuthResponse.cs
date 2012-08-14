namespace Zuliaworks.Netzuela.Spuria.Tipos
{
	using System;
	using System.Runtime.Serialization;						// DataContract, DataMember
	
	using ServiceStack.ServiceInterface.ServiceModel;		// IHasResponseStatus
	
	[DataContract]
	public class AuthResponse : IHasResponseStatus
	{
		[DataMember]
		public string SessionId { get; set; }
		[DataMember]
		public string UserName { get; set; }
		[DataMember]
		public ResponseStatus ResponseStatus { get; set; }
	}
}

