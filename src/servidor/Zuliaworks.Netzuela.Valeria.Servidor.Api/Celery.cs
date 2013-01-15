namespace Zuliaworks.Netzuela.Valeria.Servidor.Api
{
    using System;
    using System.Collections.Generic;            // List, Dictionary
    using System.Data;
    using System.Runtime.Serialization;         // DataMember, DataContract
    using System.Text;
    
    using RabbitMQ.Client;
    using ServiceStack.ServiceModel.Serialization;
    
    public static class Celery
    {
        #region Constantes
        
        private const string Uri = "amqp://guest@localhost:5672//";
        private const string Exchange = "celery";
        private const string ExchangeType = RabbitMQ.Client.ExchangeType.Direct;
        private const string Queue = "celery";
        private const string RoutingKey = "celery";
        private static readonly ConnectionFactory cf;
        
        #endregion
        
        #region Constructores
        
        static Celery()
        {
            cf = new ConnectionFactory();
            cf.Uri = Uri;
        }
        
        #endregion
        
        #region Funciones

        public static void RegistrarProducto(DataTable filasNuevas)
        {
            using (IConnection conexion = cf.CreateConnection())
            using (IModel modelo = conexion.CreateModel())
            {
                modelo.ExchangeDeclare(Exchange, ExchangeType, true);
                /*modelo.QueueDeclare(Constantes.CeleryCola, false, false, false, null);
                modelo.QueueBind(Constantes.CeleryCola, Constantes.CeleryExchange, Constantes.CeleryRoutingKey);*/
                                
                foreach (DataRow fila in filasNuevas.Rows)
                {
                    var tarea = new TareaCelery
                    {
                        task = "tareas.registrar_producto",
                        id = Guid.NewGuid().ToString(),
                        args = new List<object> { fila["codigo"] },
                        kwargs = null,
                        retries = null,
                        eta = null,
                        expires = null
                    };

                    string mensaje = JsonDataContractSerializer.Instance.SerializeToString<TareaCelery>(tarea);
                    IBasicProperties propiedades = modelo.CreateBasicProperties();
                    propiedades.ContentType = "application/json";
                    propiedades.DeliveryMode = 2;                    
                    modelo.BasicPublish(Exchange, RoutingKey, propiedades, Encoding.UTF8.GetBytes(mensaje));
                }
            }
        }
        
        #endregion
        
        #region Tipos anidados
        
        private class TareaCelery
        {
            #region Constructores
            
            public TareaCelery () { }
            
            #endregion
            
            #region Propiedades
            
            [DataMember]
            public string task { get; set; }
            [DataMember]
            public string id { get; set; }
            [DataMember]
            public List<object> args { get; set; }
            [DataMember]
            public Dictionary<string, object> kwargs { get; set; }
            [DataMember]
            public int? retries { get; set; }
            [DataMember]
            public string eta { get; set; }
            [DataMember]
            public string expires { get; set; }
            
            #endregion
        }
        
        #endregion
    }
}