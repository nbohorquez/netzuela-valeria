using System;using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections.ObjectModel;           // ObservableCollection
using Zuliaworks.Netzuela.Valeria.Logica;       // Nodo

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels
{
    /// <summary>
    /// Esta clase lleva el repositorio de todos los Nodo's que tienen un NodoViewModel asociado
    /// </summary>
    public static class NodoViewModelExtensiones
    {
        #region Variables

        private static Dictionary<Nodo, NodoViewModel> Repositorio = new Dictionary<Nodo, NodoViewModel>();

        #endregion

        #region Funciones

        public static bool ExisteEnRepositorio(this Nodo Nodo)
        {
            if (Nodo == null)
                throw new ArgumentNullException("Nodo");

            return Repositorio.ContainsKey(Nodo);
        }

        public static bool ExisteEnRepositorio(this NodoViewModel NodoVM)
        {
            if (NodoVM == null)
                throw new ArgumentNullException("NodoVM");

            return Repositorio.ContainsValue(NodoVM);
        }

        public static void AgregarARepositorio(this Nodo Nodo, NodoViewModel NodoVM)
        {
            if (Nodo == null)
                throw new ArgumentNullException("Nodo");
            else if (NodoVM == null)
                throw new ArgumentNullException("NodoVM");

            Repositorio.Add(Nodo, NodoVM);
        }

        public static bool QuitarDeRepositorio(this Nodo Nodo)
        {
            if (Nodo == null)
                throw new ArgumentNullException("Nodo");

            return Repositorio.Remove(Nodo);
        }

        public static NodoViewModel BuscarEnRepositorio(this Nodo Nodo)
        {
            if (Nodo == null)
                throw new ArgumentNullException("Nodo");

            return Repositorio[Nodo];
        }

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
                throw new ArgumentNullException("Ruta");
            else if (Arbol == null)
                throw new ArgumentNullException("Arbol");

            NodoViewModel Resultado = null;

            string[] PasosDeLaRuta = Ruta.Split('\\');

            for (int i = 0; i < (PasosDeLaRuta.Length - 1); i++)
            {
                if (i == 0)
                {
                    Resultado = NodoViewModelExtensiones.BuscarNodo(PasosDeLaRuta[i], Arbol);
                }
                else
                {
                    Resultado = NodoViewModelExtensiones.BuscarNodo(PasosDeLaRuta[i], Resultado.Hijos);
                }
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
    }
}
