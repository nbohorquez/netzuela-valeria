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
    public class MapeoDeColumnas
    {
        #region Variables

        private Nodo _ColumnaDestino;
        private Nodo _ColumnaOrigen;
        public delegate void CambioEnColumnasEvento(object Remitente, CambioEnColumnasArgumentos Argumentos); 

        #endregion

        #region Constructores

        /// <summary>
        /// Crea un mapeo de columnas vacío.
        /// </summary>
        public MapeoDeColumnas() { }

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
        public MapeoDeColumnas(Nodo ColumnaDestino, Nodo ColumnaOrigen)
        {
            this.ColumnaDestino = ColumnaDestino;
            this.ColumnaOrigen = ColumnaOrigen;
        }
        
        #endregion

        #region Propiedades

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

                    if(CambioEnColumnas != null)
                        CambioEnColumnas(this, new CambioEnColumnasArgumentos("Origen", ValorAnterior, ValorNuevo));
                }
                else if (ValorNuevo != ColumnaDestino && ValorNuevo != _ColumnaOrigen)
                {
                    if (ValorNuevo.Nivel != Constantes.NivelDeNodo.COLUMNA)
                        throw new Exception("El nodo a asociar no es de tipo columna");

                    if (TablaPadre != null)
                    {
                        if (TablaPadre.NodoEsLegal(ValorNuevo))
                        {
                            ValorNuevo.MapaColumna = this;
                            _ColumnaOrigen = ValorNuevo;

                            if (CambioEnColumnas != null)
                                CambioEnColumnas(this, new CambioEnColumnasArgumentos("Origen", ValorAnterior, ValorNuevo));
                        }
                    }
                    else
                    {
                        ValorNuevo.MapaColumna = this;
                        _ColumnaOrigen = ValorNuevo;

                        if (CambioEnColumnas != null)
                            CambioEnColumnas(this, new CambioEnColumnasArgumentos("Origen", ValorAnterior, ValorNuevo));
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

                    if(CambioEnColumnas != null)
                        CambioEnColumnas(this, new CambioEnColumnasArgumentos("Destino", ValorAnterior, ValorNuevo));
                }
                else if (ValorNuevo != ColumnaOrigen && ValorNuevo != _ColumnaDestino)
                {
                    if (ValorNuevo.Nivel != Constantes.NivelDeNodo.COLUMNA)
                        throw new Exception("El nodo a asociar no es de tipo columna");

                    if (TablaPadre != null)
                    {
                        if (TablaPadre.NodoEsLegal(ValorNuevo))
                        {
                            ValorNuevo.MapaColumna = this;
                            _ColumnaDestino = ValorNuevo;

                            if (CambioEnColumnas != null)
                                CambioEnColumnas(this, new CambioEnColumnasArgumentos("Destino", ValorAnterior, ValorNuevo));
                        }
                    }
                    else
                    {
                        ValorNuevo.MapaColumna = this;
                        _ColumnaDestino = ValorNuevo;

                        if (CambioEnColumnas != null)
                            CambioEnColumnas(this, new CambioEnColumnasArgumentos("Destino", ValorAnterior, ValorNuevo));
                    }                 
                }
            }
        }

        /// <summary>
        /// Instancia de <see cref="TablaMapeada"/> asociada a esta clase.
        /// </summary>
        public TablaMapeada TablaPadre { get; set; }

        #endregion

        #region Eventos

        public event CambioEnColumnasEvento CambioEnColumnas;

        #endregion

        #region Funciones
        
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
    }
}
