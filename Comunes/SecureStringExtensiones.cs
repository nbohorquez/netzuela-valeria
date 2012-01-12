using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.InteropServices;           // unsafe
using System.Security;                          // SecureString

namespace Zuliaworks.Netzuela.Valeria.Comunes
{
    /// <summary>
    /// Contiene funciones adicionales para los tipos SecureString.
    /// </summary>
    public static class SecureStringExtensiones
    {
        #region Funciones

        /*
         * Codigo importado
         * ================
         * 
         * Autor: Fabio Pintos
         * Titulo: How to properly convert SecureString to String
         * Licencia: DESCONOCIDA
         * Fuente: http://blogs.msdn.com/b/fpintos/archive/2009/06/12/how-to-properly-convert-securestring-to-string.aspx
         * 
         * Tipo de uso
         * ===========
         * 
         * Textual                                              []
         * Adaptado                                             []
         * Solo se cambiaron los nombres de las variables       [X]
         * 
         */

        /// <summary>
        /// 
        /// </summary>
        /// <param name="StringSegura"></param>
        /// <returns></returns>
        public static string ConvertirAUnsecureString(this SecureString StringSegura)
        {
            if (StringSegura == null)
                throw new ArgumentNullException("StringSegura");

            IntPtr StringNoSegura = IntPtr.Zero;

            try
            {
                StringNoSegura = Marshal.SecureStringToGlobalAllocUnicode(StringSegura);
                return Marshal.PtrToStringUni(StringNoSegura);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(StringNoSegura);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="StringNoSegura"></param>
        /// <returns></returns>
        public static SecureString ConvertirASecureString(this string StringNoSegura)
        {
            if (StringNoSegura == null)
                throw new ArgumentNullException("StringNoSegura");

            /*
             * Uso de la instruccion fixed
             * ============================
             * 
             * La instrucción fixed evita que el recolector de elementos no utilizados "reubique" una 
             * variable móvil. La instrucción fixed solo se permite en un contexto no seguro. Fixed 
             * también se puede utilizar para crear búferes de tamaño fijo.
             * (http://msdn.microsoft.com/es-es/library/f58wzh21%28v=VS.100%29.aspx)
             * 
             * Para habilitar la compilación de codigo no seguro, vaya a Proyecto->Propiedades->Generar y 
             * marque la casilla "Permitir codigo no seguro".
             */

            unsafe
            {
                fixed (char* Apuntador = StringNoSegura)
                {
                    var StringSegura = new SecureString(Apuntador, StringNoSegura.Length);
                    StringSegura.MakeReadOnly();
                    return StringSegura;
                }
            }
        }

        public static void AgregarString(this SecureString StringSegura, string StringAgregada)
        {
            if (StringAgregada == null)
                throw new ArgumentNullException("StringAgregada");
            if (StringSegura == null)
                throw new ArgumentNullException("StringSegura");

            for (int i = 0; i < StringAgregada.Length; i++)
            {
                StringSegura.AppendChar(StringAgregada[i]);
            }
        }

        public static void AgregarSecureString(this SecureString StringSegura, SecureString StringAgregada)
        {
            if (StringAgregada == null)
                throw new ArgumentNullException("StringAgregada");
            if (StringSegura == null)
                throw new ArgumentNullException("StringSegura");

            StringSegura.AgregarString(StringAgregada.ConvertirAUnsecureString());
        }

        #endregion
    }
}
