namespace Zuliaworks.Netzuela.Spuria.Tipos
{
	using System;
	using System.Runtime.Serialization;						// DataContract, DataMember
	
	[DataContract]
	public class Auth
	{
		[DataMember]
		public string UserName { get; set; }
		[DataMember]
		public string Password { get; set; }
		[DataMember]
		public bool? RememberMe { get; set; }
	}
}