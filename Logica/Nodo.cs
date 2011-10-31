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
    public class Nodo : INotifyPropertyChanged
    {
        #region Variables

        private MapeoDeColumnas _MapaColumna;

        /// <summary>
        /// 
        /// </summary>
        public bool Expandido;

        /// <summary>
        /// 
        /// </summary>
        public int Nivel;

        /// <summary>
        /// 
        /// </summary>
        public Nodo Padre;

        /// <summary>
        /// 
        /// </summary>
        public Explorador Explorador;

        #endregion

        #region Constructores

        /// <summary>
        /// 
        /// </summary>
        public Nodo()
        {
            this.Expandido = false;
            this.Nombre = null;
            this.Nivel = -1;
            this.Padre = null;
            this.Hijos = new ObservableCollection<Nodo>();
            this.Explorador = null;
            this.MapaColumna = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Nombre"></param>
        public Nodo(string Nombre)
        {
            this.Expandido = false;
            this.Nombre = Nombre;
            this.Nivel = -1;
            this.Padre = null;
            this.Hijos = new ObservableCollection<Nodo>() { new Nodo() };
            this.Explorador = null;
            this.MapaColumna = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Nombre"></param>
        /// <param name="Nivel"></param>
        public Nodo(string Nombre, int Nivel)
        {
            this.Expandido = false;
            this.Nombre = Nombre;
            this.Nivel = Nivel;
            this.Padre = null;
            this.Hijos = new ObservableCollection<Nodo>() { new Nodo() };
            this.Explorador = null;
            this.MapaColumna = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Nombre"></param>
        /// <param name="Padre"></param>
        public Nodo(string Nombre, Nodo Padre)
        {
            this.Expandido = false;
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
            this.Expandido = true;
            this.Nombre = Nombre;
            this.Hijos = Hijos;
            this.MapaColumna = null;
            Padre.AgregarHijo(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Nombre"></param>
        /// <param name="Padre"></param>
        /// <param name="Hijos"></param>
        public Nodo(string Nombre, Nodo Padre, string[] Hijos)
        {
            this.Expandido = true;
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

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<Nodo> Hijos { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Nombre { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public MapeoDeColumnas MapaColumna 
        {
            get { return _MapaColumna; }
            set
            {
                if (value != _MapaColumna)
                {
                    _MapaColumna = value;
                    RegistrarCambioEnPropiedad("MapaColumna");
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
        /// <param name="Hijo"></param>
        public void AgregarHijo(Nodo Hijo)
        {
            Hijo.Padre = this;
            Hijo.Nivel = this.Nivel + 1;
            Hijo.Explorador = this.Explorador;

            this.Hijos.Add(Hijo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Nombre"></param>
        /// <param name="Lista"></param>
        /// <returns></returns>
        public static Nodo BuscarNodo(string Nombre, ObservableCollection<Nodo> Lista)
        {
            Nodo Resultado = new Nodo();

            foreach (Nodo n in Lista)
            {
                if (n.Nombre == Nombre)
                {
                    Resultado = n;
                    break;
                }
            }

            return Resultado;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Nodo"></param>
        /// <returns></returns>
        public static string RutaCompleta(Nodo Nodo)
        {
            List<string> Ruta = new List<string>();

            while (Nodo != null)
            {
                Ruta.Add(Nodo.Nombre);
                Nodo = Nodo.Padre;
            }

            string Resultado = "";

            for (int i = Ruta.Count; i > 0; i--)
            {
                Resultado += Ruta[i - 1] + "\\";
            }

            return Resultado;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Nodo"></param>
        /// <returns></returns>
        public static string[] ListarHijos(Nodo Nodo)
        {
            List<string> Resultado = new List<string>();

            foreach (Nodo Hijo in Nodo.Hijos)
            {
                Resultado.Add(Hijo.Nombre);
            }

            return Resultado.ToArray();
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
