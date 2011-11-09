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
        #region Variables

        // ...

        #endregion

        #region Constructores

        /// <summary>
        /// 
        /// </summary>
        public Nodo()
        {
            //this.Expandido = false;
            this.Nombre = null;
            this.Nivel = -1;
            this.Padre = null;
            this.Hijos = new ObservableCollection<Nodo>();
            //this.Explorador = null;
            this.MapaColumna = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Nombre"></param>
        public Nodo(string Nombre)
        {
            //this.Expandido = false;
            this.Nombre = Nombre;
            this.Nivel = -1;
            this.Padre = null;
            this.Hijos = new ObservableCollection<Nodo>() { new Nodo() };
            //this.Explorador = null;
            this.MapaColumna = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Nombre"></param>
        /// <param name="Nivel"></param>
        public Nodo(string Nombre, int Nivel)
        {
            //this.Expandido = false;
            this.Nombre = Nombre;
            this.Nivel = Nivel;
            this.Padre = null;
            this.Hijos = new ObservableCollection<Nodo>() { new Nodo() };
            //this.Explorador = null;
            this.MapaColumna = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Nombre"></param>
        /// <param name="Padre"></param>
        public Nodo(string Nombre, Nodo Padre)
        {
            //this.Expandido = false;
            this.Nombre = Nombre;
            this.Hijos = new ObservableCollection<Nodo>() { new Nodo() };
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
            //this.Expandido = true;
            this.Nombre = Nombre;
            this.MapaColumna = null;
            Padre.AgregarHijo(this);
            this.Hijos = new ObservableCollection<Nodo>();

            foreach (Nodo n in this.Hijos)
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
            //this.Expandido = true;
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
        //public bool Expandido { get; set; }
        //public Explorador Explorador { get; set; }

        #endregion

        #region Eventos

        // ...

        #endregion

        #region Funciones

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Hijo"></param>
        public void AgregarHijo(Nodo Hijo)
        {
            Hijo.Padre = this;
            Hijo.Nivel = this.Nivel + 1;
            //Hijo.Explorador = this.Explorador;

            this.Hijos.Add(Hijo);
        }

        #endregion

        #region Implementaciones de interfaces

        // ...

        #endregion

        #region Tipos anidados

        // ...

        #endregion
    }
}
