namespace Zuliaworks.Netzuela.Valeria.Comunes
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;           // ObservableCollection
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Contiene funciones adicionales para los tipos IEnumerable
    /// </summary>
    public static class IEnumerableExtensiones
    {
        /// <summary>
        /// Convierte un tipo IEnumerable a ObservableCollection
        /// </summary>
        /// <typeparam name="T">Tipo de IEnumerable</typeparam>
        /// <param name="enumerable">Coleccion de objetos</param>
        /// <returns>ObservableColeccion de objetos tipo T</returns>
        public static ObservableCollection<T> ConvertirAObservableCollection<T>(this IEnumerable<T> enumerable)
        {
            var c = new ObservableCollection<T>();

            foreach (var e in enumerable)
            {
                c.Add(e);
            }

            return c;
        }

        public static List<T> ConvertirALista<T>(this IEnumerable<T> enumerable)
        {
            var c = new List<T>();

            foreach (var e in enumerable)
            {
                c.Add(e);
            }

            return c;
        }
    }
}
