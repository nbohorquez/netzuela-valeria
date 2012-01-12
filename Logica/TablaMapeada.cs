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

        private Nodo _NodoTabla;

        #endregion

        #region Constructores

        /// <summary>
        /// 
        /// </summary>
        public TablaMapeada()
        {
            NodoTabla = new Nodo();
            MapasColumnas = new List<MapeoDeColumnas>();            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Tabla"></param>
        /// <param name="Columnas"></param>
        public TablaMapeada(Nodo Tabla, List<Nodo> Columnas)
        {
            if (Tabla == null)
                throw new ArgumentNullException("Tabla");
            if (Columnas == null)
                throw new ArgumentNullException("Columnas");
            
            this.NodoTabla = Tabla;
            this.MapasColumnas = new List<MapeoDeColumnas>();

            foreach (Nodo Columna in Columnas)
            {
                MapeoDeColumnas MapCol = new MapeoDeColumnas(this, Columna);
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
        public Nodo NodoTabla
        {
            get { return _NodoTabla; }
            set
            {
                if (value != _NodoTabla)
                {
                    if (value.Nivel != Constantes.NivelDeNodo.TABLA)
                        throw new ArgumentException("El nodo tiene que ser de nivel Tabla para poder crear una TablaMapeada", "NodoTabla");
                    
                    _NodoTabla = value;
                }
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
                if (Nodo.ExisteEnRepositorio())
                {
                    if (Nodo.MapaColumna.ColumnaOrigen == Nodo)
                    {
                        Nodo.MapaColumna.QuitarOrigen();
                    }
                    else if (Nodo.MapaColumna.ColumnaDestino == Nodo)
                    {
                        Resultado = false;
                    }
                }
                else
                {
                    Nodo.AgregarARepositorio();
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
                this.MapasColumnas.Add(MapaDeColumna);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool QuitarMapa(MapeoDeColumnas MapaDeColumna)
        {
            if (MapaDeColumna == null)
                throw new ArgumentNullException("MapaDeColumna");

            if (MapasColumnas.Contains(MapaDeColumna))
            {
                return this.MapasColumnas.Remove(MapaDeColumna);
            }
            else
            {
                return false;
            }
        }

        #endregion
    }
}
