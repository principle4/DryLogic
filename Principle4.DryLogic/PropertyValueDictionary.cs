using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Principle4.DryLogic
{
  public class PropertyValueDictionary : IDictionary<String, PropertyValue>
  {
    private Dictionary<String, PropertyValue> internalDictionary { get; set; }

    public ObjectInstance ParentObjectInstance { get; private set; }

    public PropertyValueDictionary(ObjectInstance oiParent)
      
    {
      internalDictionary = new Dictionary<string, PropertyValue>(oiParent.ObjectDefinition.Properties.Count);
      ParentObjectInstance = oiParent;
    }

    public PropertyValue this[String propertyName]
    {
      get
      {
        if (!this.ContainsKey(propertyName))
        {
          var propertyValue = 
            ParentObjectInstance.ObjectDefinition.Properties[propertyName].CreatePropertyValue(ParentObjectInstance);
          this.Add(propertyName, propertyValue);
        }
        return internalDictionary[propertyName];
      }
      internal set
      {
        internalDictionary[propertyName] = value;
      }
    }


    #region IDictionary<string,PropertyValue> Members

    bool IDictionary<string, PropertyValue>.TryGetValue(string key, out PropertyValue value)
    {
      //providing TryGetValue for MVCs ModelDataBinder (accessing dictionary by key rather than by index requires it:
      //http://stackoverflow.com/a/18683004 (dotPeek shows a call to TryGetValue on IDictionary)
      //I could wrap this in a try catch to preserve the spirit of the "Try" method, but the exception that would result is
      //is not "not found" but instead meant that the key wasn't in the object definition
      value = this[key];
      return true;
    }

    PropertyValue IDictionary<string, PropertyValue>.this[string key]
    {
      get
      {
        return this[key];
      }
      set
      {
        throw new AccessViolationException("Cannot explicity set a PropertyValue object");
      }
    }

    #endregion

    #region IDictionary<string,PropertyValue> Members

    public void Add(string key, PropertyValue value)
    {
      internalDictionary.Add(key, value);
    }

    public bool ContainsKey(string key)
    {
      return internalDictionary.ContainsKey(key);
    }

    public ICollection<string> Keys
    {
      get { return internalDictionary.Keys; }
    }

    public bool Remove(string key)
    {
      return internalDictionary.Remove(key);
    }

    public ICollection<PropertyValue> Values
    {
      get { return internalDictionary.Values; }
    }

    #endregion

    #region ICollection<KeyValuePair<string,PropertyValue>> Members

    public void Add(KeyValuePair<string, PropertyValue> item)
    {
      internalDictionary.Add(item.Key, item.Value);
    }

    public void Clear()
    {
      internalDictionary.Clear();
    }

    public bool Contains(KeyValuePair<string, PropertyValue> item)
    {
      return ((ICollection<KeyValuePair<string, PropertyValue>>)internalDictionary).Contains(item);
    }

    public void CopyTo(KeyValuePair<string, PropertyValue>[] array, int arrayIndex)
    {
      ((ICollection<KeyValuePair<string, PropertyValue>>)internalDictionary).CopyTo(array, arrayIndex);
    }

    public int Count
    {
      get
      {
        return internalDictionary.Count;
      }
    }

    public bool IsReadOnly
    {
      get { throw new NotImplementedException(); }
    }

    public bool Remove(KeyValuePair<string, PropertyValue> item)
    {
      return ((ICollection<KeyValuePair<string, PropertyValue>>)internalDictionary).Remove(item);
    }

    #endregion

    #region IEnumerable<KeyValuePair<string,PropertyValue>> Members

    public IEnumerator<KeyValuePair<string, PropertyValue>> GetEnumerator()
    {
      return ((ICollection<KeyValuePair<string, PropertyValue>>)internalDictionary).GetEnumerator();
    }

    #endregion

    #region IEnumerable Members

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return ((IEnumerable)internalDictionary).GetEnumerator();
    }

    #endregion
  }
}
