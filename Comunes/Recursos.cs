using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections.ObjectModel;           // ObservableCollection
using System.ComponentModel;                    // INotifyPropertyChanged
using System.Runtime.InteropServices;           // unsafe
using System.Security;                          // SecureString

namespace Zuliaworks.Netzuela.Valeria.Comunes
{
    /// <summary>
    /// Define constantes empleadas por todas las capas de Valeria
    /// </summary>    
    public static class Constantes
    {
        /// <summary>
        /// SGBDR: Sistemas Gestores de Bases de Datos Relacionales (RDBMS en inglés)
        /// </summary>
        public class SGBDR
        {
            public const string NETZUELA = "Netzuela";          // No es realmente un SGBDR pero...
            public const string SQL_SERVER = "SQL Server";
            public const string MYSQL = "MySQL";
            public const string ORACLE = "Oracle";
            public const string ACCESS = "Access";
            public const string DB2 = "DB2";
        }

        /// <summary>
        /// Métodos de conexión
        /// </summary>        
        public class MetodosDeConexion
        {
            public const string MEMORIA_COMPARTIDA = "Memoria compartida";
            public const string CANALIZACIONES_CON_NOMBRE = "Canalizaciones con nombre";
            public const string TCP_IP = "TCP/IP";
            public const string VIA = "VIA";
        }

        /// <summary>
        /// Credenciales
        /// </summary>
        public class Credenciales
        {
            public const int CHIVO = 0;
            public const int NETZUELA = 1;
        }

        /// <summary>
        /// Credenciales
        /// </summary>
        public class NivelDeNodo
        {
            public const int NULO = -1;
            public const int RAIZ = 0;

            public const int SERVIDOR = 0;
            public const int BASE_DE_DATOS = 1;
            public const int TABLA = 2;
            public const int COLUMNA = 3;
        }
    }

    /// <summary>
    /// Contiene funciones adicionales para los tipos ObservableCollection
    /// </summary>
    public static class ColeccionesObservables
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Enumerable"></param>
        /// <returns></returns>
        public static ObservableCollection<T> ConvertirAObservableCollection<T>(this IEnumerable<T> Enumerable)
        {
            var c = new ObservableCollection<T>();
            foreach (var e in Enumerable)
                c.Add(e);
            return c;
        }
    }

    /// <summary>
    /// Contiene funciones adicionales para los tipos SecureString. Este codigo fue tomado de la pagina: 
    /// http://blogs.msdn.com/b/fpintos/archive/2009/06/12/how-to-properly-convert-securestring-to-string.aspx
    /// </summary>
    public static class StringSegura
    {
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
    }

    /// <summary>
    /// Esta clase se usa en todas partes. Lo que en verdad quiero hacer (y todavia no se como) son 
    /// 2 clases: una que implemente INotifyPropertyChanged (para ser usada en la capa de logica y 
    /// presentacion) y otra que no (para usarse en la capa de datos). Seria muy facil simplemente 
    /// hacer dos clases distintas pero es mucho mas elegante que una derive de la otra. ¿Tendeis?
    /// </summary>
    public class DatosDeConexion : INotifyPropertyChanged
    {
        #region Variables

        private string _Anfitrion = string.Empty;
        private string _Servidor = string.Empty;
        private string _Instancia = string.Empty;
        private string _MetodoDeConexion = string.Empty;
        private string _ArgumentoDeConexion = string.Empty;

        #endregion

        #region Constructores

        public DatosDeConexion() { }

        #endregion

        #region  Propiedades

        public string Anfitrion
        {
            get { return _Anfitrion; }
            set
            {
                if (value != _Anfitrion)
                {
                    _Anfitrion = value;
                    OnPropertyChanged("Anfitrion");
                }
            }
        }
        
        public string Servidor
        {
            get { return _Servidor; }
            set
            {
                if (value != _Servidor)
                {
                    _Servidor = value;
                    OnPropertyChanged("Servidor");
                }
            }
        }
        
        public string Instancia
        {
            get { return _Instancia; }
            set
            {
                if (value != _Instancia)
                {
                    _Instancia = value;
                    OnPropertyChanged("Instancia");
                }
            }
        }
        
        public string MetodoDeConexion
        {
            get { return _MetodoDeConexion; }
            set
            {
                if (value != _MetodoDeConexion)
                {
                    _MetodoDeConexion = value;
                    OnPropertyChanged("MetodoDeConexion");
                }
            }
        }
        
        public string ArgumentoDeConexion
        {
            get { return _ArgumentoDeConexion; }
            set
            {
                if (value != _ArgumentoDeConexion)
                {
                    _ArgumentoDeConexion = value;
                    OnPropertyChanged("ArgumentoDeConexion");
                }
            }
        }

        #endregion

        #region Eventos

        // ...

        #endregion

        #region Funciones o métodos

        // ...

        #endregion

        #region Implementaciones de interfaces

        /// <summary>
        /// Evento que se activa cuando una propiedad de esta clase ha sido modificada.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Esta función se llama de forma interna cuando se cambia una propiedad de esta clase
        /// </summary>
        /// <param name="info">Nombre de la propiedad modificada.</param>
        protected virtual void OnPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion

        #region Tipos anidados

        // ...        

        #endregion
    }
}
