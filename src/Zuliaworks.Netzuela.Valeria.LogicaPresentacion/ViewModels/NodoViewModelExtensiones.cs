namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels
{
    using System;
    using System.Collections.Generic;               // Dictionary
    using System.Collections.ObjectModel;           // ObservableCollection
    using System.Data;
    using System.Linq;
    using System.Text;
    
    using Zuliaworks.Netzuela.Valeria.Logica;       // Nodo

    /// <summary>
    /// Esta clase lleva el repositorio de todos los Nodo's que tienen un NodoViewModel asociado
    /// </summary>
    public static class NodoViewModelExtensiones
    {
        #region Variables

        private static Dictionary<Nodo, NodoViewModel> RepositorioDeNodos = new Dictionary<Nodo, NodoViewModel>();
        private static Dictionary<NodoViewModel, DataTable> RepositorioDeTablas = new Dictionary<NodoViewModel, DataTable>();

        #endregion

        #region Funciones

        #region RepositorioDeNodos

        public static bool ExisteEnRepositorioDeNodos(this Nodo Nodo)
        {
            if (Nodo == null)
                throw new ArgumentNullException("Nodo");

            return RepositorioDeNodos.ContainsKey(Nodo);
        }

        public static bool ExisteEnRepositorioDeNodos(this NodoViewModel NodoVM)
        {
            if (NodoVM == null)
                throw new ArgumentNullException("NodoVM");

            return RepositorioDeNodos.ContainsValue(NodoVM);
        }

        public static void AgregarARepositorioDeNodos(this Nodo Nodo, NodoViewModel NodoVM)
        {
            if (Nodo == null)
                throw new ArgumentNullException("Nodo");
            else if (NodoVM == null)
                throw new ArgumentNullException("NodoVM");

            RepositorioDeNodos.Add(Nodo, NodoVM);
        }

        public static bool QuitarDeRepositorioDeNodos(this Nodo Nodo)
        {
            if (Nodo == null)
                throw new ArgumentNullException("Nodo");

            return RepositorioDeNodos.Remove(Nodo);
        }

        public static NodoViewModel BuscarEnRepositorioDeNodos(this Nodo Nodo)
        {
            if (Nodo == null)
                throw new ArgumentNullException("Nodo");

            return RepositorioDeNodos[Nodo];
        }

        #endregion

        #region RepositorioDeTablas

        public static bool ExisteEnRepositorioDeTablas(this NodoViewModel NodoVM)
        {
            if (NodoVM == null)
                throw new ArgumentNullException("NodoVM");

            return RepositorioDeTablas.ContainsKey(NodoVM);
        }

        public static bool ExisteEnRepositorioDeTablas(this DataTable Tabla)
        {
            if (Tabla == null)
                throw new ArgumentNullException("Tabla");

            return RepositorioDeTablas.ContainsValue(Tabla);
        }

        public static void AgregarARepositorioDeTablas(this NodoViewModel NodoVM, DataTable Tabla)
        {
            if (NodoVM == null)
                throw new ArgumentNullException("NodoVM");
            if (Tabla == null)
                throw new ArgumentNullException("Tabla");

            RepositorioDeTablas.Add(NodoVM, Tabla);
        }

        public static bool QuitarDeRepositorioDeTablas(this NodoViewModel NodoVM)
        {
            if (NodoVM == null)
                throw new ArgumentNullException("NodoVM");

            return RepositorioDeTablas.Remove(NodoVM);
        }

        public static DataTable BuscarEnRepositorioDeTablas(this NodoViewModel NodoVM)
        {
            if (NodoVM == null)
                throw new ArgumentNullException("NodoVM");

            return RepositorioDeTablas[NodoVM];
        }

        #endregion

        #region Otras

        public static NodoViewModel BuscarNodo(string Nombre, ObservableCollection<NodoViewModel> Lista)
        {
            if (Nombre == null)
                throw new ArgumentNullException("Nombre");
            else if (Lista == null)
                throw new ArgumentNullException("Lista");

            NodoViewModel Resultado = null;

            foreach (NodoViewModel n in Lista)
            {
                if (n.Nombre == Nombre)
                {
                    Resultado = n;
                    break;
                }
            }

            return Resultado;
        }

        public static string RutaCompleta(this NodoViewModel Nodo)
        {
            if (Nodo == null)
                throw new ArgumentNullException("Nodo");

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

        public static NodoViewModel RutaANodo(string Ruta, ObservableCollection<NodoViewModel> Arbol)
        {
            if (Ruta == null)
            {
                throw new ArgumentNullException("Ruta");
            }
            else if (Arbol == null)
            {
                throw new ArgumentNullException("Arbol");
            }

            NodoViewModel Resultado = null;
            string[] PasosDeLaRuta = Ruta.Split('\\');
            int i = 0;

            foreach (string Paso in PasosDeLaRuta)
            {
                if (Paso == string.Empty)
                {
                    continue;
                }

                if (i == 0)
                {
                    Resultado = NodoViewModelExtensiones.BuscarNodo(Paso, Arbol);
                }
                else
                {
                    Resultado = NodoViewModelExtensiones.BuscarNodo(Paso, Resultado.Hijos);
                }
                i++;
            }

            return Resultado;
        }

        public static string[] ListarHijos(this NodoViewModel Nodo)
        {
            if (Nodo == null)
                throw new ArgumentNullException("Nodo");

            List<string> Resultado = new List<string>();

            foreach (NodoViewModel Hijo in Nodo.Hijos)
            {
                Resultado.Add(Hijo.Nombre);
            }

            return Resultado.ToArray();
        }

        #endregion

        #endregion
    }
}
