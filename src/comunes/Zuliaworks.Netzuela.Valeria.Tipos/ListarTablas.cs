namespace Zuliaworks.Netzuela.Valeria.Tipos
{
    using System;
    using System.Runtime.Serialization;     // DataContract, DataMember
    
    using ServiceStack.ServiceHost;         // RestService
    using ServiceStack.ServiceInterface;    // Authenticate

    #if (SERVIDOR)
    [Authenticate()]
    #endif
    [DataContract]
    [RestService("/listartablas")]
    public class ListarTablas
    {
        [DataMember]
        public string BaseDeDatos { get; set; }
    }
}