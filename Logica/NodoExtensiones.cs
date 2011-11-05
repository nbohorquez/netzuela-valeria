using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections.ObjectModel;           // ObservableCollection

namespace Zuliaworks.Netzuela.Valeria.Logica
{
    public static class NodoExtensiones
    {
        #region Funciones

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
        public static string RutaCompleta(this Nodo Nodo)
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
        public static string[] ListarHijos(this Nodo Nodo)
        {
            List<string> Resultado = new List<string>();

            foreach (Nodo Hijo in Nodo.Hijos)
            {
                Resultado.Add(Hijo.Nombre);
            }

            return Resultado.ToArray();
        }

        #endregion
    }
}
