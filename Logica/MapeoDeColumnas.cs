using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;        // INotifyPropertyChanged
using Zuliaworks.Netzuela.Valeria.Comunes;              // Constantes

namespace Zuliaworks.Netzuela.Valeria.Logica
{
    /// <summary>
    /// Se emplea para mapear (sic), esto es, para crear una transformación o mediación 
    /// de datos entre dos repositorios (columnas en este caso) distintos. Esta clase es 
    /// empleada por <see cref="MapeoDeTablas"/> como unidad básica de mapeo.
    /// </summary>
    public class MapeoDeColumnas : INotifyPropertyChanged
    {
        #region Variables

        /// <summary>
        /// Instancia de <see cref="MapeoDeTablas"/> asociada a esta clase.
        /// </summary>
        public MapeoDeTablas MapaTabla;
        private Nodo _ColumnaDestino;
        private Nodo _ColumnaOrigen;

        #endregion

        #region Constructores

        /// <summary>
        /// Crea un mapeo de columnas vacío.
        /// </summary>
        public MapeoDeColumnas()
        {
            this.MapaTabla = null;
            this.ColumnaDestino = null;
            this.ColumnaOrigen = null;
        }

        /// <summary>
        /// Crea un mapeo de columnas especificando sólo la columna destino.
        /// </summary>
        /// <param name="ColumnaDestino">Nodo que representa la columna destino.</param>
        public MapeoDeColumnas(Nodo ColumnaDestino)
        {
            this.MapaTabla = null;
            this.ColumnaDestino = ColumnaDestino;
            this.ColumnaOrigen = null;            
        }

        /// <summary>
        /// Crea un mapeo de columnas especificando la columna de orígen y la de destino.
        /// </summary>
        /// <param name="ColumnaDestino">Nodo que representa la columna destino.</param>
        /// <param name="ColumnaOrigen">Nodo que representa la columna orígen.</param>
        public MapeoDeColumnas(Nodo ColumnaDestino, Nodo ColumnaOrigen)
        {
            this.MapaTabla = null;
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
            set
            {
                Nodo Nueva = value as Nodo;

                if (Nueva == null)
                {
                    _ColumnaOrigen = null;
                    RegistrarCambioEnPropiedad("ColumnaOrigen");
                }
                else if (Nueva.Nivel == Constantes.NivelDeNodo.COLUMNA && Nueva != ColumnaDestino && Nueva != _ColumnaOrigen)
                {
                    if (MapaTabla != null)
                    {
                        if (MapaTabla.NodoEsLegal(Nueva))
                        {
                            Nueva.MapaColumna = this;
                            _ColumnaOrigen = Nueva;
                            RegistrarCambioEnPropiedad("ColumnaOrigen");
                        }
                    }
                    else
                    {
                        Nueva.MapaColumna = this;
                        _ColumnaOrigen = Nueva;
                        RegistrarCambioEnPropiedad("ColumnaOrigen");
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
            set
            {
                Nodo Nueva = value as Nodo;

                if (Nueva == null)
                {
                    _ColumnaDestino = null;
                    RegistrarCambioEnPropiedad("ColumnaDestino");
                }
                else if (Nueva.Nivel == Constantes.NivelDeNodo.COLUMNA && Nueva != ColumnaOrigen && Nueva != _ColumnaDestino)
                {
                    if (MapaTabla != null)
                    {
                        if (MapaTabla.NodoEsLegal(Nueva))
                        {
                            Nueva.MapaColumna = this;
                            _ColumnaDestino = Nueva;
                            RegistrarCambioEnPropiedad("ColumnaDestino");
                        }
                    }
                    else
                    {
                        Nueva.MapaColumna = this;
                        _ColumnaDestino = Nueva;
                        RegistrarCambioEnPropiedad("ColumnaDestino");
                    }                 
                }
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
        /// <param name="ColumnaOrigen"></param>
        public void Asociar(Nodo ColumnaOrigen)
        {
            this.ColumnaOrigen = ColumnaOrigen;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Desasociar()
        {
            if (this.ColumnaOrigen != null)
            {
                this.ColumnaOrigen.MapaColumna = null;
                this.ColumnaOrigen = null;
            }
        }

        #endregion

        #region Implementaciones de interfaces

        /// <summary>
        /// Evento que se activa cuando una propiedad de esta clase ha sido modificada.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Esta función se llama de forma interna cuando se cambia una propiedad de esta clase
        /// </summary>
        /// <param name="info">Nombre de la propiedad modificada.</param>
        protected virtual void RegistrarCambioEnPropiedad(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion

        #region Tipos anidados

        // ...

        #endregion
    }
}
