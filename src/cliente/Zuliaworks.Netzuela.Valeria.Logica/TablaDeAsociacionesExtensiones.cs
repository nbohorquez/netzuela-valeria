namespace Zuliaworks.Netzuela.Valeria.Logica
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class TablaDeAsociacionesExtensiones
    {
        #region Variables

        private static HashSet<Nodo> repositorio = new HashSet<Nodo>();

        #endregion

        #region Funciones

        public static void AgregarARepositorio(this Nodo nodo)
        {
            if (nodo == null)
            {
                throw new ArgumentNullException("nodo");
            }
        
            repositorio.Add(nodo);
        }

        public static bool ExisteEnRepositorio(this Nodo nodo)
        {
            if (nodo == null)
            {
                throw new ArgumentNullException("nodo");
            }

            return repositorio.Contains(nodo);
        }

        public static bool QuitarDeRepositorio(this Nodo nodo)
        {
            if (nodo == null)
            {
                throw new ArgumentNullException("nodo");
            }

            return repositorio.Remove(nodo);
        }

        #endregion
    }
}
