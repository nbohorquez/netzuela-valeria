namespace Zuliaworks.Netzuela.Valeria.Cliente.Logica
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;                    // INotifyPropertyChanged
    using System.Linq;
    using System.Text;
   
    using Zuliaworks.Netzuela.Valeria.Comunes;      // Constantes

    /// <summary>
    /// Se emplea para mapear (sic), esto es, para crear una transformación o mediación 
    /// de datos entre dos repositorios (columnas en este caso) distintos. Esta clase es 
    /// empleada por <see cref="TablaDeAsociaciones"/> como unidad básica de mapeo.
    /// </summary>
    public class AsociacionDeColumnas : Desechable
    {
        #region Variables

        private Nodo columnaDestino;
        private Nodo columnaOrigen;

        #endregion

        #region Constructores

        /// <summary>
        /// Crea un mapeo de columnas vacío.
        /// </summary>
        public AsociacionDeColumnas() { }

        public AsociacionDeColumnas(TablaDeAsociaciones tablaPadre) 
        {
            this.TablaPadre = tablaPadre;
        }

        /// <summary>
        /// Crea un mapeo de columnas especificando sólo la columna destino.
        /// </summary>
        /// <param name="columnaDestino">Nodo que representa la columna destino.</param>
        public AsociacionDeColumnas(Nodo columnaDestino)
        {
            this.ColumnaDestino = columnaDestino;
        }

        /// <summary>
        /// Crea un mapeo de columnas especificando la columna de orígen y la de destino.
        /// </summary>
        /// <param name="columnaOrigen">Nodo que representa la columna destino.</param>
        /// <param name="columnaDestino">Nodo que representa la columna orígen.</param>
        public AsociacionDeColumnas(Nodo columnaOrigen, Nodo columnaDestino)
        {
            this.ColumnaOrigen = columnaOrigen;
            this.ColumnaDestino = columnaDestino;
        }

        public AsociacionDeColumnas(TablaDeAsociaciones tablaPadre, Nodo columnaDestino)
            : this(columnaDestino)
        {
            this.TablaPadre = tablaPadre;
        }

        public AsociacionDeColumnas(TablaDeAsociaciones tablaPadre, Nodo columnaDestino, Nodo columnaOrigen)
            : this(columnaOrigen, columnaDestino)
        {
            this.TablaPadre = tablaPadre;
        }
        
        ~AsociacionDeColumnas()
        {
            this.Dispose(false);
        }

        #endregion

        #region Eventos

        public event EventHandler<EventoCambioEnColumnasArgs> CambioEnColumnas;

        #endregion

        #region Propiedades

        /// <summary>
        /// Instancia de <see cref="TablaDeAsociaciones"/> asociada a esta clase.
        /// </summary>
        public TablaDeAsociaciones TablaPadre { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Nodo ColumnaOrigen
        {
            get 
            { 
                return this.columnaOrigen; 
            }

            private set
            {
                Nodo valorNuevo = value;
                Nodo valorAnterior = this.columnaOrigen;

                if (valorNuevo == null)
                {
                    if (valorAnterior != null)
                    {
                        if (valorAnterior.ExisteEnRepositorio())
                        {
                            valorAnterior.QuitarDeRepositorio();
                        }
                    }

                    this.columnaOrigen = valorNuevo;
                    this.DispararCambioEnColumnas(new EventoCambioEnColumnasArgs("Origen", valorAnterior, valorNuevo));
                }
                else if (valorNuevo != this.ColumnaDestino && valorNuevo != this.columnaOrigen)
                {
                    if (valorNuevo.Nivel != NivelDeNodo.Columna)
                    {
                        throw new ArgumentException("El nodo tiene que ser una columna de una tabla", "nodoOrigen");
                    }

                    if (this.TablaPadre != null)
                    {
                        if (this.TablaPadre.NodoEsLegal(valorNuevo))
                        {
                            valorNuevo.Sociedad = this;
                            this.columnaOrigen = valorNuevo;
                            this.DispararCambioEnColumnas(new EventoCambioEnColumnasArgs("Origen", valorAnterior, valorNuevo));
                        }
                    }
                    else
                    {
                        valorNuevo.Sociedad = this;
                        this.columnaOrigen = valorNuevo;
                        this.DispararCambioEnColumnas(new EventoCambioEnColumnasArgs("Origen", valorAnterior, valorNuevo));
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Nodo ColumnaDestino
        {
            get 
            { 
                return this.columnaDestino; 
            }

            private set
            {
                Nodo valorNuevo = value;
                Nodo valorAnterior = this.columnaDestino;

                if (valorNuevo == null)
                {
                    if (valorAnterior != null)
                    {
                        if (valorAnterior.ExisteEnRepositorio())
                        {
                            valorAnterior.QuitarDeRepositorio();
                        }
                    }

                    this.columnaDestino = valorNuevo;
                    this.DispararCambioEnColumnas(new EventoCambioEnColumnasArgs("Destino", valorAnterior, valorNuevo));
                }
                else if (valorNuevo != this.ColumnaOrigen && valorNuevo != this.columnaDestino)
                {
                    if (valorNuevo.Nivel != NivelDeNodo.Columna)
                    {
                        throw new ArgumentException("El nodo tiene que ser una columna de una tabla", "nodoDestino");
                    }

                    if (this.TablaPadre != null)
                    {
                        if (this.TablaPadre.NodoEsLegal(valorNuevo))
                        {
                            valorNuevo.Sociedad = this;
                            this.columnaDestino = valorNuevo;
                            this.DispararCambioEnColumnas(new EventoCambioEnColumnasArgs("Destino", valorAnterior, valorNuevo));
                        }
                    }
                    else
                    {
                        valorNuevo.Sociedad = this;
                        this.columnaDestino = valorNuevo;
                        this.DispararCambioEnColumnas(new EventoCambioEnColumnasArgs("Destino", valorAnterior, valorNuevo));
                    }
                }
            }
        }
        
        #endregion

        #region Funciones
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnaOrigen"></param>
        public void FijarOrigen(Nodo columnaOrigen)
        {
            if (columnaOrigen == null)
            {
                throw new ArgumentNullException("ColumnaOrigen");
            }

            try
            {
                this.ColumnaOrigen = columnaOrigen;
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo fijar la columna de origen en esta AsociacionDeColumnas", ex);
            }
        }

        public void FijarDestino(Nodo columnaDestino)
        {
            if (columnaDestino == null)
            {
                throw new ArgumentNullException("ColumnaDestino");
            }

            try
            {
                this.ColumnaDestino = columnaDestino;
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo fijar la columna de destino en esta AsociacionDeColumnas", ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void QuitarOrigen()
        {
            try
            {
                //this.ColumnaOrigen.Sociedad = null;
                this.ColumnaOrigen = null;
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo quitar la columna de origen de esta AsociacionDeColumnas", ex);
            }
        }

        public void QuitarDestino()
        {
            try
            {
                //this.ColumnaDestino.Sociedad = null;
                this.ColumnaDestino = null;
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo quitar la columna de destino de esta AsociacionDeColumnas", ex);
            }
        }

        protected virtual void DispararCambioEnColumnas(EventoCambioEnColumnasArgs argumentos)
        {
            if (this.CambioEnColumnas != null)
            {
                this.CambioEnColumnas(this, argumentos);
            }
        }

        protected override void Dispose(bool borrarCodigoAdministrado)
        {
            if (this.TablaPadre != null)
            {
                int i = this.TablaPadre.Sociedades.IndexOf(this);
                this.TablaPadre.Sociedades[i] = null;
                this.TablaPadre = null;
            }

            if (this.ColumnaDestino != null)
            {
                this.QuitarDestino();
            }

            if (this.ColumnaOrigen != null)
            {
                this.QuitarOrigen();
            }
        }

        #endregion
    }
}
