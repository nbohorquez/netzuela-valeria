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
            MapasColumnas = new List<MapeoDeColumnas>();
            NodoTabla = new Nodo();
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

            this.NodoTabla = Tabla;
            this.MapasColumnas = new List<MapeoDeColumnas>();

            foreach (Nodo Columna in Columnas)
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
        public Nodo NodoTabla
        {
            get { return _NodoTabla; }
            private set
            {
                Nodo Nuevo = value as Nodo;
                if (Nuevo.Nivel == Constantes.NivelDeNodo.TABLA && Nuevo != _NodoTabla)
                    _NodoTabla = Nuevo;
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
