using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;                              // ConnectionState
using System.Globalization;                     // Culture
using System.Windows.Data;                      // IValueConverter, IMultiValueConverter

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion
{
    /// <summary>
    /// Este convertidor lo uso para transformar ConnectionState a palabras
    /// </summary>
    public class EstadoDeConexion : IValueConverter
    {
        public object Convert(object Valor, Type TipoObjetivo, object Parametro, CultureInfo Cultura)
        {
            ConnectionState Estado = (ConnectionState)Valor;
            object Resultado = null;

            switch (Estado)
            {
                case ConnectionState.Broken:
                    Resultado = "Conexión: Rota";
                    break;
                case ConnectionState.Closed:
                    Resultado = "Conexión: Cerrada";
                    break;
                case ConnectionState.Connecting:
                    Resultado = "Conexión: Conectando...";
                    break;
                case ConnectionState.Executing:
                    Resultado = "Conexión: Ejecutando";
                    break;
                case ConnectionState.Fetching:
                    Resultado = "Conexión: Recibiendo";
                    break;
                case ConnectionState.Open:
                    Resultado = "Conexión: Establecida";
                    break;
                default:
                    break;
            }
            return Resultado;
        }

        public object ConvertBack(object Valor, Type TipoObjetivo, object Parametro, CultureInfo Cultura)
        {
            throw new NotImplementedException();
        }
    }
}
