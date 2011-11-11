using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections.ObjectModel;           // ObservableCollection

namespace Zuliaworks.Netzuela.Valeria.Comunes
{
    /// <summary>
    /// Contiene funciones adicionales para los tipos IEnumerable
    /// </summary>
    public static class IEnumerableExtensiones
    {
        /// <summary>
        /// Convierte un tipo IEnumerable a ObservableCollection
        /// </summary>
        /// <typeparam name="T">Tipo de IEnumerable</typeparam>
        /// <param name="Enumerable">Coleccion de objetos</param>
        /// <returns>ObservableColeccion de objetos tipo T</returns>
        public static ObservableCollection<T> ConvertirAObservableCollection<T>(this IEnumerable<T> Enumerable)
        {
            var c = new ObservableCollection<T>();

            foreach (var e in Enumerable)
                c.Add(e);

            return c;
        }
    }
}
