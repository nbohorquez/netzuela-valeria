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
            return Repositorio.ContainsKey(Nodo);
        }

        public static bool ExisteEnRepositorio(this NodoViewModel NodoVidewModel)
        {
            return Repositorio.ContainsValue(NodoVidewModel);
        }

        public static void AgregarARepositorio(this Nodo Nodo, NodoViewModel NodoViewModel)
        {
            Repositorio.Add(Nodo, NodoViewModel);
        }

        public static bool QuitarDeRepositorio(this Nodo Nodo)
        {
            return Repositorio.Remove(Nodo);
        }

        public static NodoViewModel BuscarEnRepositorio(this Nodo Nodo)
        {
            return Repositorio[Nodo];
        }

        public static NodoViewModel BuscarNodo(string Nombre, ObservableCollection<NodoViewModel> Lista)
        {
            NodoViewModel Resultado = new NodoViewModel();

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

        public static string[] ListarHijos(this NodoViewModel Nodo)
        {
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
