namespace Zuliaworks.Netzuela.Valeria.Logica
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;           // ObservableCollection
    using System.Linq;
    using System.Text;

    using Zuliaworks.Netzuela.Valeria.Comunes;      // Constantes

    /// <summary>
    /// 
    /// </summary>
    public class TablaDeAsociaciones : Desechable
    {
        #region Variables

        private Nodo nodoTabla;

        #endregion

        #region Constructores

        /// <summary>
        /// 
        /// </summary>
        public TablaDeAsociaciones()
        {
            this.NodoTabla = new Nodo();
            this.Sociedades = new List<AsociacionDeColumnas>();            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tabla"></param>
        /// <param name="columnas"></param>
        public TablaDeAsociaciones(Nodo tabla, List<Nodo> columnas)
        {
            if (tabla == null)
            {
                throw new ArgumentNullException("tabla");
            }

            if (columnas == null)
            {
                throw new ArgumentNullException("columnas");
            }
            
            this.NodoTabla = tabla;
            this.Sociedades = new List<AsociacionDeColumnas>();

            foreach (Nodo columna in columnas)
            {
                AsociacionDeColumnas sociedad = new AsociacionDeColumnas(this, columna);
                this.AgregarMapa(sociedad);
            }
        }

        ~TablaDeAsociaciones()
        {
            this.Dispose(false);
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
            get 
            { 
                return this.nodoTabla; 
            }

            set
            {
                if (value != this.nodoTabla)
                {
                    if (value != null)
                    {
                        if (value.Nivel != NivelDeNodo.Tabla)
                        {
                            throw new ArgumentException("El nodo tiene que ser de nivel Tabla para poder crear una TablaDeAsociaciones", "NodoTabla");
                        }
                    }

                    this.nodoTabla = value;
                }
            }
        }

        #endregion

        #region Funciones

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodo"></param>
        /// <returns></returns>
        public bool NodoEsLegal(Nodo nodo)
        {
            bool resultado = true;

            if (nodo != null)
            {
                if (nodo.ExisteEnRepositorio())
                {
                    if (nodo.Sociedad.ColumnaOrigen == nodo)
                    {
                        nodo.Sociedad.QuitarOrigen();
                    }
                    else if (nodo.Sociedad.ColumnaDestino == nodo)
                    {
                        resultado = false;
                    }
                }
                else
                {
                    nodo.AgregarARepositorio();
                }
            }
                        
            return resultado;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapaDeColumna"></param>
        /// <returns></returns>
        public bool AgregarMapa(AsociacionDeColumnas mapaDeColumna)
        {
            if (mapaDeColumna == null)
            {
                throw new ArgumentNullException("mapaDeColumna");
            }

            if (this.NodoEsLegal(mapaDeColumna.ColumnaDestino) && this.NodoEsLegal(mapaDeColumna.ColumnaOrigen))
            {
                this.Sociedades.Add(mapaDeColumna);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool QuitarMapa(AsociacionDeColumnas sociedad)
        {
            if (sociedad == null)
            {
                throw new ArgumentNullException("sociedad");
            }

            if (this.Sociedades.Contains(sociedad))
            {
                return this.Sociedades.Remove(sociedad);
            }
            else
            {
                return false;
            }
        }

        protected override void Dispose(bool borrarCodigoAdministrado)
        {
            this.NodoTabla = null;

            if (borrarCodigoAdministrado)
            {
                if (this.Sociedades != null)
                {
                    for (int i = 0; i < this.Sociedades.Count; i++)
                    {
                        if (this.Sociedades[i] != null)
                        {
                            this.Sociedades[i].Dispose();
                        }
                    }

                    this.Sociedades.Clear();
                    this.Sociedades = null;
                }
            }
        }

        #endregion
    }
}
