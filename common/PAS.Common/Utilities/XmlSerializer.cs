using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace PAS.Common.Utilities
{
    public static class XmlSerializer<T> where T : class
    {
        public static T DeSerialize(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                return null;
            }
            xml = xml.Trim('\r', '\n', ' ');
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            XmlTextReader reader = new XmlTextReader(new System.IO.StringReader(xml));
            return serializer.Deserialize(reader) as T;
        }

        public static string Serialize(T obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }

            XmlSerializer serializer = new XmlSerializer(typeof(T));
            StringBuilder output = new StringBuilder();
            XmlTextWriter writer = new XmlTextWriter(new System.IO.StringWriter(output));
            serializer.Serialize(writer, obj);

            return output.ToString();
        }
    }
}
