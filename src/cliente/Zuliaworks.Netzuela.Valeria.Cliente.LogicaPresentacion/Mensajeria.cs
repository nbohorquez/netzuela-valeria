namespace Zuliaworks.Netzuela.Valeria.Cliente.LogicaPresentacion
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using MvvmFoundation.Wpf;       // Messenger

    public static class Mensajeria
    {
        #region Variables y Constantes

        private readonly static Messenger mensajero = new Messenger();
        internal const string GuardarConfiguracion = "Guardar configuracion";
        internal const string CargarConfiguracion = "Cargar configuracion";
        internal const string ConfiguracionGuardada = "Configuracion guardada";
        internal const string ConfiguracionCargada = "Configuracion cargada";
        internal const string TiendaSeleccionada = "Tienda seleccionada";

        #endregion

        #region Propiedades

        internal static Messenger Mensajero
        {
            get { return mensajero; }
        }

        #endregion
    }
}
