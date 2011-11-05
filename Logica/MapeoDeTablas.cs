using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections.ObjectModel;           // ObservableCollection
using System.Data;                              // ConnectionState, DataTable
using System.Collections;
using Zuliaworks.Netzuela.Valeria.Comunes;                          // Constantes

namespace Zuliaworks.Netzuela.Valeria.Logica
{
    /// <summary>
    /// 
    /// </summary>
    public class MapeoDeTablas
    {
        #region Variables

        private List<MapeoDeColumnas> _MapasColumnas;
        private Nodo _Tabla;

        #endregion

        #region Constructores

        /// <summary>
        /// 
        /// </summary>
        public MapeoDeTablas()
        {
            _MapasColumnas = new List<MapeoDeColumnas>();
            Tabla = new Nodo();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Tabla"></param>
        public MapeoDeTablas(Nodo Tabla)
        {
            this.Tabla = Tabla;
            this._MapasColumnas = new List<MapeoDeColumnas>();

            foreach (Nodo Columna in this.Tabla.Hijos)
            {
                MapeoDeColumnas MapCol = new MapeoDeColumnas(Columna);
                AgregarMapa(MapCol);
            }
        }

        #endregion

        #region Propiedades

        /// <summary>
        /// 
        /// </summary>
        public List<MapeoDeColumnas> MapasColumnas 
        {
            get { return _MapasColumnas; }
        }

        /// <summary>
        /// 
        /// </summary>
        public Nodo Tabla
        {
            get { return _Tabla; }
            set
            {
                Nodo Nuevo = value as Nodo;
                if (Nuevo.Nivel == Constantes.NivelDeNodo.TABLA && Nuevo != _Tabla)
                    _Tabla = Nuevo;
            }
        }

        #endregion

        #region Eventos

        // ...

        #endregion

        #region Funciones

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Nodo"></param>
        /// <returns></returns>
        public bool NodoEsLegal(Nodo Nodo)
        {
            bool Resultado = true;

            if (Nodo != null)
            {
                foreach (MapeoDeColumnas Mapa in MapasColumnas)
                {
                    // Si hay una columna de origen igual a la nueva que se quiere agregar, se elimina la vieja
                    if (Mapa.ColumnaOrigen != null)
                    {
                        if (Mapa.ColumnaOrigen.Nombre == Nodo.Nombre)
                        {
                            string RutaNodoNuevo = Nodo.RutaCompleta();
                            string RutaNodoViejo = Mapa.ColumnaOrigen.RutaCompleta();

                            if (RutaNodoNuevo == RutaNodoViejo)
                            {
                                Mapa.Desasociar();
                            }
                        }
                    }

                    if (Mapa.ColumnaDestino != null)
                    {
                        // Definitivamente en una tabla destino no puede haber dos columnas iguales
                        if (Mapa.ColumnaDestino.Nombre == Nodo.Nombre)
                        {
                            Resultado = false;
                            break;
                        }
                    }
                }
            }

            return Resultado;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="MapaDeColumna"></param>
        /// <returns></returns>
        public bool AgregarMapa(MapeoDeColumnas MapaDeColumna)
        {
            if (NodoEsLegal(MapaDeColumna.ColumnaDestino) && NodoEsLegal(MapaDeColumna.ColumnaOrigen))
            {
                MapaDeColumna.MapaTabla = this;
                MapasColumnas.Add(MapaDeColumna);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DataTable TablaMapeada()
        {
            DataTable TempTablaMapeada = new DataTable();

            foreach (MapeoDeColumnas MapaCol in MapasColumnas)
            {
                DataColumn TablaColSinTipo = new DataColumn(MapaCol.ColumnaDestino.Nombre);
                TempTablaMapeada.Columns.Add(TablaColSinTipo);

                if (MapaCol.ColumnaOrigen != null)
                {
                    DataTable Temp = MapaCol.ColumnaOrigen.Explorador.ObtenerTabla(MapaCol.ColumnaOrigen.Padre);
                    DataColumn TempCol = Temp.Columns[MapaCol.ColumnaOrigen.Nombre];

                    TempTablaMapeada.Columns.Remove(MapaCol.ColumnaDestino.Nombre);

                    DataColumn TablaColConTipo = new DataColumn(MapaCol.ColumnaDestino.Nombre, TempCol.DataType);
                    TempTablaMapeada.Columns.Add(TablaColConTipo);

                    while (TempTablaMapeada.Rows.Count < Temp.Rows.Count)
                    {
                        TempTablaMapeada.Rows.Add(TempTablaMapeada.NewRow());
                    }

                    for (int i = 0; i < Temp.Rows.Count; i++)
                    {
                        TempTablaMapeada.Rows[i][TablaColConTipo.ColumnName] = Temp.Rows[i][TempCol.ColumnName];
                    }
                }
            }

            return TempTablaMapeada;
        }

        #endregion

        #region Implementaciones de interfaces

        // ...

        #endregion

        #region Tipos anidados

        // ...

        #endregion
    }
}
