using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;                    // INotifyPropertyChanged
using Zuliaworks.Netzuela.Valeria.Comunes;      // Constantes

namespace Zuliaworks.Netzuela.Valeria.Logica
{
    /// <summary>
    /// Se emplea para mapear (sic), esto es, para crear una transformación o mediación 
    /// de datos entre dos repositorios (columnas en este caso) distintos. Esta clase es 
    /// empleada por <see cref="TablaMapeada"/> como unidad básica de mapeo.
    /// </summary>
    public class MapeoDeColumnas : IDisposable
    {
        #region Variables

        private TablaMapeada _TablaPadre;
        private Nodo _ColumnaDestino;
        private Nodo _ColumnaOrigen;

        #endregion

        #region Constructores

        /// <summary>
        /// Crea un mapeo de columnas vacío.
        /// </summary>
        public MapeoDeColumnas() { }

        public MapeoDeColumnas(TablaMapeada TablaPadre) 
        {
            this.TablaPadre = TablaPadre;
        }

        /// <summary>
        /// Crea un mapeo de columnas especificando sólo la columna destino.
        /// </summary>
        /// <param name="ColumnaDestino">Nodo que representa la columna destino.</param>
        public MapeoDeColumnas(Nodo ColumnaDestino)
        {
            this.ColumnaDestino = ColumnaDestino;
        }

        /// <summary>
        /// Crea un mapeo de columnas especificando la columna de orígen y la de destino.
        /// </summary>
        /// <param name="ColumnaDestino">Nodo que representa la columna destino.</param>
        /// <param name="ColumnaOrigen">Nodo que representa la columna orígen.</param>
        public MapeoDeColumnas(Nodo ColumnaOrigen, Nodo ColumnaDestino)
        {
            this.ColumnaOrigen = ColumnaOrigen;
            this.ColumnaDestino = ColumnaDestino;
        }

        public MapeoDeColumnas(TablaMapeada TablaPadre, Nodo ColumnaDestino)
            : this(ColumnaDestino)
        {
            this.TablaPadre = TablaPadre;
        }

        public MapeoDeColumnas(TablaMapeada TablaPadre, Nodo ColumnaDestino, Nodo ColumnaOrigen)
            : this(ColumnaOrigen, ColumnaDestino)
        {
            this.TablaPadre = TablaPadre;
        }
        
        ~MapeoDeColumnas()
        {
            Dispose(false);
        }

        #endregion

        #region Propiedades

        /// <summary>
        /// Instancia de <see cref="TablaMapeada"/> asociada a esta clase.
        /// </summary>
        public TablaMapeada TablaPadre { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Nodo ColumnaOrigen
        {
            get { return _ColumnaOrigen; }
            private set
            {
                Nodo ValorNuevo = value;
                Nodo ValorAnterior = _ColumnaOrigen;

                if (ValorNuevo == null)
                {
                    _ColumnaOrigen = ValorNuevo;

                    DispararCambioEnColumnas(new EventoCambioEnColumnasArgs("Origen", ValorAnterior, ValorNuevo));
                }
                else if (ValorNuevo != ColumnaDestino && ValorNuevo != _ColumnaOrigen)
                {
                    if (ValorNuevo.Nivel != Constantes.NivelDeNodo.COLUMNA)
                        throw new ArgumentException("El nodo tiene que ser una columna de una tabla", "NodoOrigen");

                    if (TablaPadre != null)
                    {
                        if (TablaPadre.NodoEsLegal(ValorNuevo))
                        {
                            ValorNuevo.MapaColumna = this;
                            _ColumnaOrigen = ValorNuevo;

                            DispararCambioEnColumnas(new EventoCambioEnColumnasArgs("Origen", ValorAnterior, ValorNuevo));
                        }
                    }
                    else
                    {
                        ValorNuevo.MapaColumna = this;
                        _ColumnaOrigen = ValorNuevo;

                        DispararCambioEnColumnas(new EventoCambioEnColumnasArgs("Origen", ValorAnterior, ValorNuevo));
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Nodo ColumnaDestino
        {
            get { return _ColumnaDestino; }
            private set
            {
                Nodo ValorNuevo = value;
                Nodo ValorAnterior = _ColumnaDestino;

                if (ValorNuevo == null)
                {
                    _ColumnaDestino = ValorNuevo;

                    DispararCambioEnColumnas(new EventoCambioEnColumnasArgs("Destino", ValorAnterior, ValorNuevo));
                }
                else if (ValorNuevo != ColumnaOrigen && ValorNuevo != _ColumnaDestino)
                {
                    if (ValorNuevo.Nivel != Constantes.NivelDeNodo.COLUMNA)
                        throw new ArgumentException("El nodo tiene que ser una columna de una tabla", "NodoOrigen");

                    if (TablaPadre != null)
                    {
                        if (TablaPadre.NodoEsLegal(ValorNuevo))
                        {
                            ValorNuevo.MapaColumna = this;
                            _ColumnaDestino = ValorNuevo;

                            DispararCambioEnColumnas(new EventoCambioEnColumnasArgs("Destino", ValorAnterior, ValorNuevo));
                        }
                    }
                    else
                    {
                        ValorNuevo.MapaColumna = this;
                        _ColumnaDestino = ValorNuevo;

                        DispararCambioEnColumnas(new EventoCambioEnColumnasArgs("Destino", ValorAnterior, ValorNuevo));
                    }
                }
            }
        }
        
        #endregion

        #region Eventos

        public event EventHandler<EventoCambioEnColumnasArgs> CambioEnColumnas;

        #endregion

        #region Funciones
        
        protected virtual void DispararCambioEnColumnas(EventoCambioEnColumnasArgs Argumentos)
        {
            if(CambioEnColumnas != null)
            {
                CambioEnColumnas(this, Argumentos);
            }
        }
        
        protected void Dispose(bool BorrarCodigoAdministrado)
        {
            if (TablaPadre != null)
            {
                TablaPadre.MapasColumnas.Remove(this);
                TablaPadre = null;
            }

            if (ColumnaDestino != null)
                QuitarDestino();

            if (ColumnaOrigen != null)
                QuitarOrigen();

            if (BorrarCodigoAdministrado) { }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ColumnaOrigen"></param>
        public void FijarOrigen(Nodo ColumnaOrigen)
        {
            if (ColumnaOrigen == null)
                throw new ArgumentNullException("ColumnaOrigen");

            this.ColumnaOrigen = ColumnaOrigen;
        }

        public void FijarDestino(Nodo ColumnaDestino)
        {
            if (ColumnaDestino == null)
                throw new ArgumentNullException("ColumnaDestino");

            this.ColumnaDestino = ColumnaDestino;
        }

        /// <summary>
        /// 
        /// </summary>
        public void QuitarOrigen()
        {
            if (this.ColumnaOrigen != null)
            {
                this.ColumnaOrigen.MapaColumna = null;
                this.ColumnaOrigen = null;
            }
        }

        public void QuitarDestino()
        {
            if (this.ColumnaDestino != null)
            {
                this.ColumnaDestino.MapaColumna = null;
                this.ColumnaDestino = null;
            }
        }

        #endregion

        #region Implementacion de interfaces

        public void Dispose()
        {
            // En este enlace esta la mejor explicacion acerca de como implementar IDisposable
            // http://stackoverflow.com/questions/538060/proper-use-of-the-idisposable-interface

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
