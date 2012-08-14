namespace Zuliaworks.Netzuela.Spuria.Api
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    public static class Permisos
    {
        #region Variables

        private static readonly Dictionary<string, DescriptorDeTabla[]> entidadesPermitidas;

        #endregion

        #region Constructores

        static Permisos()
        {
            entidadesPermitidas = new Dictionary<string, DescriptorDeTabla[]>();

            DescriptorDeTabla inventarioTienda = new DescriptorDeTabla(
                "inventario_tienda",
                new string[] { "tienda_id", "codigo", "descripcion", "precio", "cantidad" },
                new string[] { "tienda_id", "codigo" },
                "tienda_id");

            entidadesPermitidas.Add(Constantes.BaseDeDatos, new DescriptorDeTabla[] { inventarioTienda });
        }

        #endregion

        #region Propiedades

        public static Dictionary<string, DescriptorDeTabla[]> EntidadesPermitidas
        {
            get { return entidadesPermitidas; }
        }

        #endregion

        #region Tipos anidados

        public class DescriptorDeTabla
        {
            #region Variables

            private readonly string nombre;
            private readonly string[] columnas;
            private readonly string[] clavePrimaria;
            private readonly string tiendaId;

            #endregion

            #region Constructores

            public DescriptorDeTabla(string nombre, string[] columnas, string[] clavePrimaria, string tiendaId)
            {
                this.nombre = nombre;
                this.columnas = columnas;
                this.clavePrimaria = clavePrimaria;
                this.tiendaId = tiendaId;
            }

            #endregion

            #region Propiedades

            public string Nombre 
            {
                get { return this.nombre; }
            }

            public string[] Columnas
            {
                get { return this.columnas; }
            }

            public string[] ClavePrimaria
            {
                get { return this.clavePrimaria; }
            }

            public string TiendaId
            {
                get { return this.tiendaId; }
            }

            #endregion
        }

        #endregion
    }
}