namespace Zuliaworks.Netzuela.Valeria.Datos
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;

    using Zuliaworks.Netzuela.Valeria.Datos.Eventos;

    public interface INetzuela : IBaseDeDatos
    {
        #region Eventos

        event EventHandler<EventoListarTiendasCompletadoArgs> ListarTiendasCompletado;

        #endregion

        #region Prototipos de funciones

        #region Metodos sincronicos

        string[] ListarTiendas();
        DataTable LeerTabla(int tiendaId, string baseDeDatos, string tabla);
        bool EscribirTabla(int tiendaId, string baseDeDatos, string nombreTabla, DataTable tabla);

        #endregion

        #region Metodos asincronicos

        void ListarTiendasAsinc();
        void LeerTablaAsinc(int tiendaId, string baseDeDatos, string tabla);
        void EscribirTablaAsinc(int tiendaId, string baseDeDatos, string nombreTabla, DataTable tabla);
        
        #endregion

        #endregion
    }
}
