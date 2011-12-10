using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion
{
    public static class ManejoDeExcepciones
    {
        public static string MostrarPilaDeExcepciones(this Exception ex)
        {
            List<string> Resultado = new List<string>();

            do
            {
                Resultado.Add(ex.Message);

            }
            while (ex.InnerException != null);

            return string.Concat(Resultado.ToArray());
        }
    }
}
