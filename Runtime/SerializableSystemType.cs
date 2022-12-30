// Simple helper class that allows you to serialize System.Type objects.
// Use it however you like, but crediting or even just contacting the author would be appreciated (Always nice to see people using your stuff!)
//
// Written by Bryan Keiren (http://www.bryankeiren.com)

using UnityEngine;

[System.Serializable]
public class SerializableSystemType
{
    [SerializeField] private string m_Name;
    [SerializeField] private string m_AssemblyQualifiedName;
    [SerializeField] private string m_AssemblyName;
    private System.Type m_Type;

    public string Name => m_Name;
    public string AssemblyQualifiedName => m_AssemblyQualifiedName;
    public string AssemblyName => m_AssemblyName;

    public System.Type Type
    {
        get
        {
            if (m_Type == null)
            {
                m_Type = System.Type.GetType(m_AssemblyQualifiedName);
            }
            return m_Type;
        }
        set
        {
            m_Type = value;
            m_Name = value.Name;
            m_AssemblyQualifiedName = value.AssemblyQualifiedName;
            m_AssemblyName = value.Assembly.FullName;
        }
    }

    public SerializableSystemType(System.Type type)
    {
        m_Type = type;
        m_Name = type.Name;
        m_AssemblyQualifiedName = type.AssemblyQualifiedName;
        m_AssemblyName = type.Assembly.FullName;
    }

    public override bool Equals(System.Object obj)
    {
        SerializableSystemType temp = obj as SerializableSystemType;
        if ((object)temp == null)
        {
            return false;
        }
        return this.Equals(temp);
    }

    public bool Equals(SerializableSystemType obj)
    {
        //return m_AssemblyQualifiedName.Equals(_Object.m_AssemblyQualifiedName);
        return obj.Type.Equals(Type);
    }

    public override int GetHashCode()
    {
        return m_Type != null ? m_Type.GetHashCode() : 0;
    }

    public static bool operator ==(SerializableSystemType a, SerializableSystemType b)
    {
        // If both are null, or both are same instance, return true.
        if (System.Object.ReferenceEquals(a, b))
        {
            return true;
        }

        // If one is null, but not both, return false.
        if (((object)a == null) || ((object)b == null))
        {
            return false;
        }

        return a.Equals(b);
    }

    public static bool operator !=(SerializableSystemType a, SerializableSystemType b)
    {
        return !(a == b);
    }
}