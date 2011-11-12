using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections.ObjectModel;           // ObservableCollection
using System.Collections;
using Zuliaworks.Netzuela.Valeria.Comunes;      // Constantes

namespace Zuliaworks.Netzuela.Valeria.Logica
{
    /// <summary>
    /// 
    /// </summary>
    public class TablaMapeada
    {
        #region Variables

        private Nodo _Tabla;

        #endregion

        #region Constructores

        /// <summary>
        /// 
        /// </summary>
        public TablaMapeada()
        {
            MapasColumnas = new List<MapeoDeColumnas>();
            Tabla = new Nodo();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Tabla"></param>
        public TablaMapeada(Nodo Tabla)
        {
            if (Tabla == null)
                throw new ArgumentNullException("Tabla");

            this.Tabla = Tabla;
            this.MapasColumnas = new List<MapeoDeColumnas>();

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
        public List<MapeoDeColumnas> MapasColumnas { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public Nodo Tabla
        {
            get { return _Tabla; }
            private set
            {
                Nodo Nuevo = value as Nodo;
                if (Nuevo.Nivel == Constantes.NivelDeNodo.TABLA && Nuevo != _Tabla)
                    _Tabla = Nuevo;
            }
        }

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
            if (MapaDeColumna == null)
                throw new ArgumentNullException("MapaDeColumna");

            if (NodoEsLegal(MapaDeColumna.ColumnaDestino) && NodoEsLegal(MapaDeColumna.ColumnaOrigen))
            {
                MapaDeColumna.TablaPadre = this;
                this.MapasColumnas.Add(MapaDeColumna);
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion
    }
}
