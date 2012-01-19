using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zuliaworks.Netzuela.Valeria.Logica
{
    public static class TablaDeAsociacionesExtensiones
    {
        #region Variables

        private static HashSet<Nodo> Repositorio = new HashSet<Nodo>();

        #endregion

        #region Funciones

        public static void AgregarARepositorio(this Nodo Nodo)
        {
            if(Nodo == null)
                throw new ArgumentNullException("Nodo");
        
            Repositorio.Add(Nodo);
        }

        public static bool ExisteEnRepositorio(this Nodo Nodo)
        {
            if (Nodo == null)
                throw new ArgumentNullException("Nodo");

            return Repositorio.Contains(Nodo);
        }

        public static bool QuitarDeRepositorio(this Nodo Nodo)
        {
            if (Nodo == null)
                throw new ArgumentNullException("Nodo");

            return Repositorio.Remove(Nodo);
        }

        #endregion
    }
}
