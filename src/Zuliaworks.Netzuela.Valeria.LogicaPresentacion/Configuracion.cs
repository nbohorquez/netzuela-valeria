 namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using System.Configuration;                                         // ConfigurationManager
    using System.Security;                                              // SecureString
    using Zuliaworks.Netzuela.Valeria.Comunes;                          // ParametrosDeConexion
    using Zuliaworks.Netzuela.Valeria.Logica;                           // TablaMapeada, MapeoDeColumnas
    using Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels;    // TablaMapeada, MapeoDeColumnas
    using Zuliaworks.Netzuela.Valeria.Preferencias;                     // CargarGuardar

    public class Configuracion
    {
        #region Constructores

        public Configuracion() 
        { 
        }

        #endregion

        #region Propiedades

        public ParametrosDeConexion ParametrosConexionLocal { get; set; }
        public ParametrosDeConexion ParametrosConexionRemota { get; set; }
        public SecureString UsuarioLocal { get; set; }
        public SecureString ContrasenaLocal { get; set; }
        public int TiendaId { get; set; }
        public SecureString UsuarioRemoto { get; set; }
        public SecureString ContrasenaRemota { get; set; }
        public List<string[]> Asociaciones { get; set; }
        public List<TablaDeAsociaciones> Tablas { get; set; }

        #endregion
    }
}