using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Web.Services;                          // InvokeAsync
using Zuliaworks.Netzuela.Paris.ContratoValeria;    // DataSetXML

namespace Zuliaworks.Netzuela.Valeria.Datos.Web
{
    public class ServidorValeriaAsinc
    {
        // Ver: http://msdn.microsoft.com/en-us/library/wewwczdw.aspx

        #region Variables

        //public delegate void EnviarTablasCompletadoEvento(object Remitente, EnviarTablasCompletadoArgumentos Argumentos);
        
        #endregion

        #region Constructores

        public ServidorValeriaAsinc()
        {}

        #endregion

        #region Propiedades

        public bool Ocupado { get; private set; }

        #endregion

        #region Eventos

        public EventHandler<EventoEnviarTablasCompletadoArgs> EnviarTablasCompletado;
        //public event EnviarTablasCompletadoEvento EnviarTablasCompletado;

        #endregion

        #region Funciones

        // Metodos sincronicos
        public bool EnviarTablas(DataSetXML Tablas);

        // Metodos asincronicos
        public void EnviarTablasAsinc(DataSetXML Tablas)
        {
            EnviarTablasAsinc(Tablas, null);
        }

        public void EnviarTablasAsinc(DataSetXML Tablas, object UsuarioID)
        {
        }        

        public void CancelarEnviarTablas(object EstadoUsuario);

        #endregion
    }
}
