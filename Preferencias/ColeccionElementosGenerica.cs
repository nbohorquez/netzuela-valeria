namespace Zuliaworks.Netzuela.Valeria.Preferencias
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Text;

    public class ColeccionElementosGenerica<T> : ConfigurationElementCollection, IEnumerable<T>, ICollection<T> where T : ConfigurationElement, new()
    {
        /* 
         * Codigo importado
         * ================
         * 
         * Autor: abatishchev
         * Titulo: How to implement a ConfigurationSection with a ConfigurationElementCollection 
         *      (pregunta en el foro "stackoverflow")
         * Licencia: Creative Commons Attribution-ShareAlike 3.0 Unported
         * Fuente: http://stackoverflow.com/questions/3935331/how-to-implement-a-configurationsection-with-a-configurationelementcollection
         * 
         * Tipo de uso
         * ===========
         * 
         * Textual                                              []
         * Adaptado                                             [X]
         * Solo se cambiaron los nombres de las variables       []
         * 
         */

        #region Variables

        private List<T> elementos = new List<T>();

        #endregion

        public new bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public ConfigurationElement this[int indice]
        {
            get
            {
                return (T)this.BaseGet(indice);
            }

            set
            {
                if (this.BaseGet(indice) != null)
                {
                    this.BaseRemoveAt(indice);
                }

                this.BaseAdd(indice, value);
            }
        }
        
        #region Funciones

        public new IEnumerator<T> GetEnumerator()
        {
            return this.elementos.GetEnumerator();
        }

        public void Add(T item)
        {
            this.BaseAdd(item);
        }

        public void Clear()
        {
            this.BaseClear();
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }
        
        public bool Remove(T item)
        {
            bool resultado = false;
            T nuevo = this.elementos.Find(e => e.Equals(item));

            if (nuevo != null)
            {
                BaseRemove(nuevo);
                resultado = true;
            }

            return resultado;
        }        

        protected override ConfigurationElement CreateNewElement()
        {
            T nuevo = new T();
            this.elementos.Add(nuevo);
            return nuevo;
        }

        protected override object GetElementKey(ConfigurationElement elemento)
        {
            // Cualquier verga...
            return elemento.GetHashCode();
        }

        #endregion
    }
}