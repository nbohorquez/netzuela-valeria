﻿using System;
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

            Resultado.Add("==========================================================\n");
            Resultado.Add("Pila de excepciones:\n\n");
            for (int i = 0; ; ex = ex.InnerException, i++)
            {
                Resultado.Add("EXCEPCION NIVEL " + i.ToString() + ": ");
                Resultado.Add(ex.Source + ".dll: \"" + ex.Message + "\"" + "\n");

                if (ex.InnerException == null)
                {
                    Resultado.Add("==========================================================\n");
                    break;
                }
            }

            Resultado.Add(ex.StackTrace);

            return string.Concat(Resultado.ToArray());
        }
    }
}
