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
    public class TablaDeAsociaciones
    {
        #region Variables

        private Nodo _NodoTabla;

        #endregion

        #region Constructores

        /// <summary>
        /// 
        /// </summary>
        public TablaDeAsociaciones()
        {
            NodoTabla = new Nodo();
            Sociedades = new List<AsociacionDeColumnas>();            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Tabla"></param>
        /// <param name="Columnas"></param>
        public TablaDeAsociaciones(Nodo Tabla, List<Nodo> Columnas)
        {
            if (Tabla == null)
                throw new ArgumentNullException("Tabla");
            if (Columnas == null)
                throw new ArgumentNullException("Columnas");
            
            this.NodoTabla = Tabla;
            this.Sociedades = new List<AsociacionDeColumnas>();

            foreach (Nodo Columna in Columnas)
            {
                AsociacionDeColumnas Sociedad = new AsociacionDeColumnas(this, Columna);
                AgregarMapa(Sociedad);
            }
        }

        #endregion

        #region Propiedades

        /// <summary>
        /// 
        /// </summary>
        public List<AsociacionDeColumnas> Sociedades { get; private set; }

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
                        throw new ArgumentException("El nodo tiene que ser de nivel Tabla para poder crear una TablaDeAsociaciones", "NodoTabla");
                    
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
                    if (Nodo.Sociedad.ColumnaOrigen == Nodo)
                    {
                        Nodo.Sociedad.QuitarOrigen();
                    }
                    else if (Nodo.Sociedad.ColumnaDestino == Nodo)
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
        public bool AgregarMapa(AsociacionDeColumnas MapaDeColumna)
        {
            if (MapaDeColumna == null)
                throw new ArgumentNullException("MapaDeColumna");

            if (NodoEsLegal(MapaDeColumna.ColumnaDestino) && NodoEsLegal(MapaDeColumna.ColumnaOrigen))
            {
                this.Sociedades.Add(MapaDeColumna);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool QuitarMapa(AsociacionDeColumnas Sociedad)
        {
            if (Sociedad == null)
                throw new ArgumentNullException("MapaDeColumna");

            if (Sociedades.Contains(Sociedad))
            {
                return this.Sociedades.Remove(Sociedad);
            }
            else
            {
                return false;
            }
        }

        #endregion
    }
}
