using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerializableDictionaryExample : MonoBehaviour
{
    // The dictionaries can be accessed throught a property
    [SerializeField]
    StringStringDictionary m_stringStringDictionary;

    public IDictionary<string, string> StringStringDictionary
    {
        get { return m_stringStringDictionary; }
        set { m_stringStringDictionary.CopyFrom(value); }
    }

    public ObjectColorDictionary m_objectColorDictionary;
    public StringColorArrayDictionary m_stringColorArrayDictionary;
    public QuaternionMyClassDictionary m_quaternionMyClassDictionary;
    public TrashSortTypeTrashDataListDictionary m_trashSortTypeTrashDataListDictionary;

    void Reset()
    {
        // access by property
        // StringStringDictionary = new Dictionary<string, string>() { { "first key", "value A" }, { "second key", "value B" }, { "third key", "value C" } };
        // m_objectColorDictionary = new ObjectColorDictionary() { { gameObject, Color.blue }, { this, Color.red } };
        // m_quaternionMyClassDictionary = new QuaternionMyClassDictionary() { { new Quaternion(1, 2, 3, 4), new MyClass(2, "Test") } };
    }
}