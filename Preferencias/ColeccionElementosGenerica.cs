using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Configuration;

namespace Zuliaworks.Netzuela.Valeria.Preferencias
{
    public class ColeccionElementosGenerica<T> : ConfigurationElementCollection, IEnumerable<T>, ICollection<T> where T : ConfigurationElement, new()
    {
        // Con codigo de http://stackoverflow.com/questions/3935331/how-to-implement-a-configurationsection-with-a-configurationelementcollection
        #region Variables

        private List<T> _Elementos = new List<T>();

        #endregion

        #region Funciones

        protected override ConfigurationElement CreateNewElement()
        {
            T Nuevo = new T();
            _Elementos.Add(Nuevo);
            return Nuevo;
        }

        protected override object GetElementKey(ConfigurationElement Elemento)
        {
            // Cualquier verga...
            return Elemento.GetHashCode();
        }

        public new IEnumerator<T> GetEnumerator()
        {
            return _Elementos.GetEnumerator();
        }

        public void Add(T item)
        {
            BaseAdd(item);
        }

        public void Clear()
        {
            BaseClear();
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public new bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(T item)
        {
            bool Resultado = false;
            T Nuevo = _Elementos.Find(e => e.Equals(item));

            if (Nuevo != null)
            {
                BaseRemove(Nuevo);
                Resultado = true;
            }

            return Resultado;
        }

        public ConfigurationElement this[int Indice]
        {
            get { return (T)base.BaseGet(Indice); }
            set
            {
                if (base.BaseGet(Indice) != null)
                {
                    base.BaseRemoveAt(Indice);
                }
                base.BaseAdd(Indice, value);
            }
        }

        #endregion
    }
}