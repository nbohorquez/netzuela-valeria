using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WcfSamples.DynamicProxy;                      // DynamicProxyFactory, DynamicObject
using System.Reflection;                            // Assembly, Type

namespace Zuliaworks.Netzuela.Valeria.Datos.Web
{
    public class DataSetXMLDinamico : DynamicObject
    {
        const string Tipo = "Zuliaworks.Netzuela.Paris.ContratoValeria.DataSetXML";
        const string EsquemaXMLPropiedad = "EsquemaXML";
        const string XMLPropiedad = "XML";

        #region Constructores

        public DataSetXMLDinamico(Assembly Ensamblado)
            : this(GetType(Ensamblado))
        {
        }

        public DataSetXMLDinamico(Type TipoDelEmpleado)
            : base(TipoDelEmpleado)
        {
            CallConstructor();
        }

        public DataSetXMLDinamico(object Datasetxml)
            : base(Datasetxml)
        {
        }

        #endregion

        #region Propiedades

        public string EsquemaXML
        {
            get
            {
                return (string)GetProperty(EsquemaXMLPropiedad);
            }

            set
            {
                SetProperty(EsquemaXMLPropiedad, value);
            }
        }

        public string XML
        {
            get
            {
                return (string)GetProperty(XMLPropiedad);
            }

            set
            {
                SetProperty(XMLPropiedad, value);
            }
        }

        #endregion

        #region Funciones

        public static Type GetType(Assembly Ensamblado)
        {
            return Ensamblado.GetType(Tipo, true, true);
        }

        #endregion
    }
}
