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
            this.Padre = null;
            this.Hijos = new ObservableCollection<Nodo>();
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
            this.Padre = null;
            this.Hijos = new ObservableCollection<Nodo>();
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
            this.Padre = null;
            this.Hijos = new ObservableCollection<Nodo>();
            this.MapaColumna = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Nombre"></param>
        /// <param name="Padre"></param>
        public Nodo(string Nombre, Nodo Padre)
        {
            if (Padre == null)
                throw new ArgumentNullException("Padre");

            this.Nombre = Nombre;
            this.Hijos = new ObservableCollection<Nodo>();
            this.MapaColumna = null;
            Padre.AgregarHijo(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Nombre"></param>
        /// <param name="Padre"></param>
        /// <param name="Hijos"></param>
        public Nodo(string Nombre, Nodo Padre, ObservableCollection<Nodo> Hijos)
        {
            if (Padre == null)
                throw new ArgumentNullException("Padre");

            this.Nombre = Nombre;
            this.MapaColumna = null;
            Padre.AgregarHijo(this);
            this.Hijos = new ObservableCollection<Nodo>();

            foreach (Nodo n in Hijos)
            {
                this.AgregarHijo(n);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Nombre"></param>
        /// <param name="Padre"></param>
        /// <param name="Hijos"></param>
        public Nodo(string Nombre, Nodo Padre, string[] Hijos)
        {
            if (Padre == null)
                throw new ArgumentNullException("Padre");
            
            this.Nombre = Nombre;
            Padre.Hijos.Add(this);
            this.Hijos = new ObservableCollection<Nodo>();
            this.MapaColumna = null;

            foreach (string Hijo in Hijos)
            {
                Nodo N = new Nodo(Hijo, this);
            }
        }

        #endregion

        #region Propiedades

        public string Nombre { get; set; }
        public int Nivel { get; set; }
        public Nodo Padre { get; set; }
        public ObservableCollection<Nodo> Hijos { get; set; }
        public MapeoDeColumnas MapaColumna { get; set; }

        #endregion

        #region Funciones

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Hijo"></param>
        public void AgregarHijo(Nodo Hijo)
        {
            if (Hijo == null)
                throw new ArgumentNullException("Hijo");

            Hijo.Padre = this;
            Hijo.Nivel = this.Nivel + 1;

            this.Hijos.Add(Hijo);
        }

        #endregion
    }
}
