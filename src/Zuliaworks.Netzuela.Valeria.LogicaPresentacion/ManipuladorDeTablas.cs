using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using Zuliaworks.Netzuela.Valeria.Logica;
using Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels;

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion
{
    public static class ManipuladorDeTablas
    {
        #region Funciones

        public static void ActualizarTabla(DataTable Tabla, TablaDeAsociaciones TabAso)
        {
            // Si te parece que uso demasiadas variables y que estoy consumiendo mucha memoria,
            // lee este articulo que explica la forma en la que C# maneja las objetos nuevos y las
            // referencias a estos objetos:
            // http://www.albahari.com/valuevsreftypes.aspx

            // Como borrar la referencia del DataGrid a un DataTable:
            // http://social.msdn.microsoft.com/Forums/en/wpf/thread/a5767cf4-8d26-4f72-b1b1-feca26bb6b2e

            if (Tabla == null)
                throw new ArgumentNullException("Tabla");
            if (TabAso == null)
                throw new ArgumentNullException("TabAso");

            using (DataTable TablaOrigen = CrearTabla(TabAso))
            {
                // Le agregamos la misma clave primaria de Tabla a TablaOrigen
                List<DataColumn> Columnas = new List<DataColumn>();

                foreach (DataColumn Columna in Tabla.PrimaryKey)
                    Columnas.Add(TablaOrigen.Columns[Columna.Ordinal]);

                TablaOrigen.PrimaryKey = Columnas.ToArray();

                // Cuando llamo a Merge() DataRowState no se actualiza automaticamente, hay que hacerlo de forma manual
                // http://social.msdn.microsoft.com/Forums/en/csharpgeneral/thread/fcf5793f-1ccf-4dce-96cd-3052f9347a8b

                // Buscamos las filas cambiadas
                var Cambiadas = from FilaOrigen in TablaOrigen.AsEnumerable()
                                from FilaDestino in Tabla.AsEnumerable()
                                where TablaOrigen.PrimaryKey.Aggregate(true, (same, keycol) => same & FilaOrigen[keycol].Equals(FilaDestino[keycol.Ordinal]))
                                && !FilaOrigen.ItemArray.SequenceEqual(FilaDestino.ItemArray)
                                select new {
                                    FO = FilaOrigen,
                                    FD = FilaDestino
                                };

                List<DataRow> CambiadasOrigen = new List<DataRow>();
                List<DataRow> CambiadasDestino = new List<DataRow>();

                foreach (var Fila in Cambiadas)
                {
                    CambiadasOrigen.Add(Fila.FO);
                    CambiadasDestino.Add(Fila.FD);
                }

                // Buscamos las filas agregadas
                List<DataRow> Agregados = TablaOrigen.AsEnumerable().Except(Tabla.AsEnumerable(), DataRowComparer.Default).Except(CambiadasOrigen, DataRowComparer.Default).ToList();

                // Buscamos las filas eliminadas
                List<DataRow> Eliminadas = Tabla.AsEnumerable().Except(TablaOrigen.AsEnumerable(), DataRowComparer.Default).Except(CambiadasDestino, DataRowComparer.Default).ToList();

                Tabla.Merge(TablaOrigen, false, MissingSchemaAction.Error);
                
                if (Eliminadas.Count > 0)
                {
                    // ¿Por que cuando llamo a Delete() pasa de una vez al estado Detached en lugar de Deleted?
                    // http://social.msdn.microsoft.com/Forums/en-US/adodotnetdataproviders/thread/cf1b16ef-a980-427f-9e4a-ceccd962b046/

                    List<DataRow> MarcarComoEliminadas = (from Fila in Tabla.AsEnumerable()
                                                          from Eliminada in Eliminadas
                                                          where Fila.ItemArray.SequenceEqual(Eliminada.ItemArray)
                                                          select Fila).ToList();
                    
                    foreach (DataRow Fila in MarcarComoEliminadas)
                        Fila.Delete();
                }
                
                if (CambiadasDestino.Count > 0)
                {
                    List<DataRow> MarcarComoCambiadas = (from Fila in Tabla.AsEnumerable()
                                                         from Cambiada in CambiadasDestino
                                                         where Fila.RowState != DataRowState.Deleted
                                                         && Fila.ItemArray.SequenceEqual(Cambiada.ItemArray)
                                                         select Fila).ToList();

                    foreach (DataRow Fila in MarcarComoCambiadas)
                        Fila.SetModified();
                }

                if (Agregados.Count > 0)
                {
                    List<DataRow> MarcarComoAgregadas = (from Fila in Tabla.AsEnumerable()
                                                         from Agregado in Agregados
                                                         where Fila.RowState != DataRowState.Deleted
                                                         && Fila.ItemArray.SequenceEqual(Agregado.ItemArray)
                                                         select Fila).ToList();

                    foreach (DataRow Fila in MarcarComoAgregadas)
                        Fila.SetAdded();
                }
            }
        }

        public static void IntegrarTabla(DataTable Tabla, TablaDeAsociaciones TabAso)
        {
            if (Tabla == null)
                throw new ArgumentNullException("Tabla");
            if (TabAso == null)
                throw new ArgumentNullException("TabAso");

            foreach (AsociacionDeColumnas Sociedad in TabAso.Sociedades)
            {
                if (Sociedad.ColumnaOrigen != null)
                {
                    NodoViewModel ColumnaOrigen = Sociedad.ColumnaOrigen.BuscarEnRepositorio();
                    NodoViewModel ColumnaDestino = Sociedad.ColumnaDestino.BuscarEnRepositorio();

                    DataTable TablaOrigen = ColumnaOrigen.Explorador.ObtenerTablaDeCache(ColumnaOrigen.Padre);

                    for (int i = 0; i < TablaOrigen.Rows.Count; i++)
                    {
                        if (Tabla.Rows.Count < TablaOrigen.Rows.Count)
                        {
                            DataRow FilaNueva = Tabla.NewRow();
                            FilaNueva[ColumnaDestino.Nombre] = TablaOrigen.Rows[i][ColumnaOrigen.Nombre];
                            Tabla.Rows.Add(FilaNueva);
                        }
                        else
                        {
                            Tabla.Rows[i][ColumnaDestino.Nombre] = TablaOrigen.Rows[i][ColumnaOrigen.Nombre];
                        }
                    }
                }
            }

            Tabla.AcceptChanges();
        }

        public static DataTable CrearTabla(TablaDeAsociaciones Tabla)
        {
            if (Tabla == null)
                throw new ArgumentNullException("Tabla");

            NodoViewModel NodoTablaDestino = Tabla.NodoTabla.BuscarEnRepositorio();
            DataTable TablaDestino = NodoTablaDestino.Explorador.ObtenerTablaDeCache(NodoTablaDestino);
            DataTable Resultado = new DataTable(Tabla.NodoTabla.Nombre);

            NodoTablaDestino = null;

            try
            {
                foreach (AsociacionDeColumnas Sociedad in Tabla.Sociedades)
                {
                    DataColumn ColDestino = new DataColumn(Sociedad.ColumnaDestino.Nombre, TablaDestino.Columns[Sociedad.ColumnaDestino.Nombre].DataType);
                    Resultado.Columns.Add(ColDestino);

                    ColDestino = null;

                    if (Sociedad.ColumnaOrigen != null)
                    {
                        NodoViewModel NodoColOrigen = Sociedad.ColumnaOrigen.BuscarEnRepositorio();
                        DataTable TablaOrigen = NodoColOrigen.Explorador.ObtenerTablaDeCache(NodoColOrigen.Padre);

                        for (int i = 0; i < TablaOrigen.Rows.Count; i++)
                        {
                            if (Resultado.Rows.Count < TablaOrigen.Rows.Count)
                            {
                                Resultado.Rows.Add(Resultado.NewRow());
                            }

                            Resultado.Rows[i][Sociedad.ColumnaDestino.Nombre] = TablaOrigen.Rows[i][Sociedad.ColumnaOrigen.Nombre];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear la tabla a partir de la TablaDeAsociaciones", ex);
            }

            Resultado.AcceptChanges();

            return Resultado;
        }

        #endregion
    }
}
