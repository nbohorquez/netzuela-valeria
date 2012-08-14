namespace Zuliaworks.Netzuela.Spuria.Tipos
{
    using System;
    using System.Collections.Generic;
    using System.Data;                          // DataRowState, DataColumn
    using System.Linq;
    using System.Runtime.Serialization;         // DataMember, DataContract
    using System.ServiceModel;
    using System.ServiceModel.Web;
    using System.Text;
		
    [DataContract(Namespace = Constantes.Namespace)]
    public class DataTableXml
    {
        #region Constructores

        public DataTableXml()
        { 
        }

        public DataTableXml(string baseDeDatos, string nombreTabla, string esquemaXml, string xml)
        {
            this.BaseDeDatos = baseDeDatos;
            this.NombreTabla = nombreTabla;
            this.EsquemaXml = esquemaXml;
            this.Xml = xml;
        }
		
        #endregion

        #region Propiedades

        [DataMember]
        public string BaseDeDatos { get; set; }
        [DataMember]
        public string NombreTabla { get; set; }
        [DataMember]
        public string EsquemaXml { get; set; }
        [DataMember]
        public string Xml { get; set; }

        #endregion
    }
}
