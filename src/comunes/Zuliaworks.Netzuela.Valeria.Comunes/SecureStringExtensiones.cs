namespace Zuliaworks.Netzuela.Valeria.Comunes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;           // unsafe
    using System.Security;                          // SecureString
    using System.Text;

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
        /// Convierte una SecureString a una string simple de forma segura.
        /// </summary>
        /// <param name="stringSegura">Cadena de caracteres segura a convertir.</param>
        /// <returns>Cadena de caracteres convertida.</returns>
        public static string ConvertirAUnsecureString(this SecureString stringSegura)
        {
            if (stringSegura == null)
            {
                throw new ArgumentNullException("stringSegura");
            }

            IntPtr stringNoSegura = IntPtr.Zero;

            try
            {
                stringNoSegura = Marshal.SecureStringToGlobalAllocUnicode(stringSegura);
                return Marshal.PtrToStringUni(stringNoSegura);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(stringNoSegura);
            }
        }

        /// <summary>
        /// Convierte una string simple a una SecureString de forma segura.
        /// </summary>
        /// <param name="stringNoSegura">Cadena de caracteres no seguros a convertir.</param>
        /// <returns>Cadena de caracteres convertida.</returns>
        public static SecureString ConvertirASecureString(this string stringNoSegura)
        {
            if (stringNoSegura == null)
            {
                throw new ArgumentNullException("stringNoSegura");
            }

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
                fixed (char* apuntador = stringNoSegura)
                {
                    var stringSegura = new SecureString(apuntador, stringNoSegura.Length);
                    stringSegura.MakeReadOnly();
                    return stringSegura;
                }
            }
        }

        public static void AgregarString(this SecureString stringSegura, string stringAgregada)
        {
            if (stringAgregada == null)
            {
                throw new ArgumentNullException("stringAgregada");
            }

            if (stringSegura == null)
            {
                throw new ArgumentNullException("stringSegura");
            }

            for (int i = 0; i < stringAgregada.Length; i++)
            {
                stringSegura.AppendChar(stringAgregada[i]);
            }
        }

        public static void AgregarSecureString(this SecureString stringSegura, SecureString stringAgregada)
        {
            if (stringAgregada == null)
            {
                throw new ArgumentNullException("stringAgregada");
            }

            if (stringSegura == null)
            {
                throw new ArgumentNullException("stringSegura");
            }

            stringSegura.AgregarString(stringAgregada.ConvertirAUnsecureString());
        }

        #endregion
    }
}
