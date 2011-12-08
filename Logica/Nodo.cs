using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections.ObjectModel;           // ObservableCollection
using System.ComponentModel;                    // INotifyPropertyChanged

namespace Zuliaworks.Netzuela.Valeria.Logica
{
    /// <summary>
    /// Implementación de la unidad básica de información de una estructura de datos 
    /// en forma de árbol. Se usa para representar la organizacion interna de las 
    /// bases de datos.
    /// </summary>
    public class Nodo
    {
        #region Constructores

        /// <summary>
        /// 
        /// </summary>
        public Nodo()
        {
            this.Nombre = null;
            this.Nivel = -1;
            this.MapaColumna = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Nombre"></param>
        public Nodo(string Nombre)
        {
            this.Nombre = Nombre;
            this.Nivel = -1;
            this.MapaColumna = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Nombre"></param>
        /// <param name="Nivel"></param>
        public Nodo(string Nombre, int Nivel)
        {
            this.Nombre = Nombre;
            this.Nivel = Nivel;
            this.MapaColumna = null;
        }
                    
        #endregion

        #region Propiedades

        public string Nombre { get; set; }
        public int Nivel { get; set; }
        public MapeoDeColumnas MapaColumna { get; set; }

        #endregion
    }
}
