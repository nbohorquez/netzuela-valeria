namespace Zuliaworks.Netzuela.Spuria.Tipos
{
    using System;
    using System.Collections.Generic;
    using System.Data;                  // DataTable, DataSet, DataRowCollection, DataRowState
    using System.IO;                    // MemoryStream
    using System.Linq;
    using System.Text;
    
    public static class DataTableXmlExtensiones
    {
        #region Funciones

        public static DataTable XmlADataTable(this DataTableXml tablaXml)
        {
            /*
             * Con codigo de: http://pstaev.blogspot.com/2008/04/passing-dataset-to-wcf-method.html
             */

            DataTable tabla = new DataTable(tablaXml.NombreTabla);

            try
            {
				tabla.ReadXmlSchema(new MemoryStream(Encoding.UTF8.GetBytes(tablaXml.EsquemaXml)));
                tabla.ReadXml(new MemoryStream(Encoding.UTF8.GetBytes(tablaXml.Xml)));
            }
            catch (Exception ex)
            {
                throw new Exception("Error al convertir el DataTableXML a un DataTable", ex);
            }

            return tabla;
        }

        public static DataTableXml DataTableAXml(this DataTable tabla, string baseDeDatos, string nombreTabla)
        {
            DataTableXml datosAEnviar = null;

            try
            {
				Stream xml = new MemoryStream();
				Stream esquemaXml = new MemoryStream();
				
				tabla.TableName = nombreTabla;
				tabla.WriteXml(xml, XmlWriteMode.DiffGram);
				tabla.WriteXmlSchema(esquemaXml);
				
				// Como hubo escritura en el intermedio, es necesario reiniciar la posicion del lector.
				xml.Position = 0;
				esquemaXml.Position = 0;
				
				StreamReader lectorXml = new StreamReader(xml, Encoding.UTF8);
				StreamReader lectorEsquemaXml = new StreamReader(esquemaXml, Encoding.UTF8);				
				
                datosAEnviar = new DataTableXml(baseDeDatos, nombreTabla, lectorEsquemaXml.ReadToEnd(), lectorXml.ReadToEnd());
            }
            catch (Exception ex)
            {
                throw new Exception("Error al convertir el DataTable a un DataTableXML", ex);
            }

            return datosAEnviar;
        }
		
        #endregion
    }
}
