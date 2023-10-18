using System;
using System.Collections;
using System.Text;

namespace TRTA.OSP.Authentication.Service.DTO
{
    /// <summary>Simple parser for X.500/LDAP Distinguished Names</summary>
    /// <remarks>see RFC 1779 for parsing info.  Values that contain certain special characters
    /// are hex-encoded per the RFC spec, and decoded automatically.  Hex-encoded binary
    /// data blobs can be retrieved as a byte[] array using <see cref="M:Thomson.TRTA.Shared.SSO.Utility.X500Name.GetBinaryParameter(System.String)" />.</remarks>
    public class X500Name
    {
        /// <summary>
        /// DN can use commas, semicolons or forward slashes as the delimiter for values; names end with '='
        /// </summary>
        private static readonly char[] ElementNameAndValueDelimiters = new char[4]
        {
      ',',
      ';',
      '/',
      '='
        };
        /// <summary>
        /// special chars that must be backslash-escaped if found in either names or values
        /// </summary>
        private static readonly char[] EscapedSpecialChars = new char[10]
        {
      ',',
      '+',
      '=',
      '"',
      '\r',
      '<',
      '>',
      '#',
      ';',
      '/'
        };
        /// <summary>
        /// whitespace chars that can be skipped as not part of either name or value
        /// </summary>
        private static readonly char[] WhiteSpace = new char[3]
        {
      ' ',
      '\t',
      '\r'
        };
        private ArrayList _elements = ArrayList.Synchronized(new ArrayList());
        private string _name;

        /// <summary>The full x.500 distinguished name</summary>
        public string DistinguishedName
        {
            get
            {
                if (this._name != null)
                    return this._name;
                StringBuilder stringBuilder = new StringBuilder(128);
                foreach (string element in this._elements)
                {
                    if (stringBuilder.Length != 0)
                        stringBuilder.Append(", ");
                    stringBuilder.Append(element);
                }
                return stringBuilder.ToString();
            }
            set
            {
                string s = value;
                if (s == null)
                    throw new ArgumentNullException("value", "X.500/LDAP Distinguished name cannot be null.");
                this._name = s;
                this._elements.Clear();
                for (; s.Length > 0; s = s.TrimStart(X500Name.WhiteSpace).TrimStart(X500Name.ElementNameAndValueDelimiters))
                {
                    string elementString1 = this.ParseElementString(ref s);
                    s = s.TrimStart(X500Name.WhiteSpace);
                    if ((int)s[0] != 61)
                        throw new ArgumentException("Invalid x.500/LDAP Distinguished Name syntax", "value");
                    s = s.Substring(1).TrimStart(X500Name.WhiteSpace);
                    string elementString2 = this.ParseElementString(ref s);
                    this.SetParameter(elementString1, elementString2);
                }
            }
        }

        /// <summary>Return a particular subelement of the DN</summary>
        /// <param name="sElement">element name to retrieve, ex. "ou"</param>
        /// <returns>value of the specified element, or empty string if not found</returns>
        public string this[string sElement]
        {
            get
            {
                return this.GetParameter(sElement);
            }
            set
            {
                this.SetParameter(sElement, value);
            }
        }

        /// <summary>Value of the Organization element in the DN  (o=...)</summary>
        public string Organization
        {
            get
            {
                return this.GetParameter("o");
            }
            set
            {
                this.SetParameter("o", value);
            }
        }

        /// <summary>
        /// Value of the Organizational Unit element of the DN (ou=...)
        /// </summary>
        public string OrganizationalUnit
        {
            get
            {
                return this.GetParameter("ou");
            }
            set
            {
                this.SetParameter("ou", value);
            }
        }

        /// <summary>Value of the UserID element of the DN  (uid=...)</summary>
        public string UserID
        {
            get
            {
                return this.GetParameter("uid");
            }
            set
            {
                this.SetParameter("uid", value);
            }
        }

        /// <summary>
        /// Value of the userPassword in the DN  (userPassword=...)
        /// </summary>
        public string UserPassword
        {
            get
            {
                return this.GetParameter("userPassword");
            }
            set
            {
                this.SetParameter("userPassword", value);
            }
        }

        /// <summary>
        /// Return a collection of x.500 element keys found in the distinguished name
        /// </summary>
        public ICollection Elements
        {
            get
            {
                ArrayList arrayList = new ArrayList(this._elements.Count);
                foreach (string element in this._elements)
                    arrayList.Add(element.Substring(0, element.IndexOf('=')));
                return (ICollection)arrayList;
            }
        }

        /// <summary>Default constructor, creates an empty name</summary>
        public X500Name()
        {
        }

        /// <summary>Constructor to load from an existing x500 string</summary>
        /// <param name="dn">x500 distinguished name string</param>
        public X500Name(string dn)
        {
            this.DistinguishedName = dn;
        }

        /// <summary>Simple copy constructor</summary>
        /// <param name="name"></param>
        public X500Name(X500Name name)
        {
            if (name == null)
                return;
            this.DistinguishedName = name.DistinguishedName;
        }

        /// <summary>
        /// Copy and modify constructor; starts with an initial X500Name then modifies it based
        /// on a collection of elements passed in the second parameter
        /// </summary>
        /// <param name="name">initial X500Name</param>
        /// <param name="additionalElements">ICollection collection of strings having the format "{elementName}={elementValue}",
        /// ex. "employeeNumber=123456ABC"</param>
        public X500Name(X500Name name, ICollection additionalElements)
        {
            if (name != null)
                this.DistinguishedName = name.DistinguishedName;
            if (additionalElements == null)
                return;
            this._name = (string)null;
            foreach (string additionalElement in (IEnumerable)additionalElements)
            {
                int length = additionalElement.IndexOf('=');
                if (length <= 0)
                    throw new ArgumentException("additionalElements does not have the proper format {key}={value}");
                this.SetParameter(additionalElement.Substring(0, length), additionalElement.Substring(length + 1));
            }
        }

        /// <summary>
        /// Copy and modify constructor; starts with an initial X500Name then modifies it based
        /// on a collection of elements passed in the second parameter
        /// </summary>
        /// <param name="name">initial X500Name</param>
        /// <param name="additionalElements">IDictionary collection of key/values containing the x500 elements</param>
        public X500Name(X500Name name, IDictionary additionalElements)
        {
            if (name != null)
                this.DistinguishedName = name.DistinguishedName;
            if (additionalElements == null)
                return;
            this._name = (string)null;
            foreach (string key in (IEnumerable)additionalElements.Keys)
                this.SetParameter(key, (string)additionalElements[key]);
        }

        /// <summary>
        /// Create an X500Name based on a collection of elements passed in
        /// </summary>
        /// <param name="x500Elements">IDictionary collection of key/values containing the x500 elements</param>
        public X500Name(IDictionary x500Elements)
        {
            if (x500Elements == null)
                return;
            foreach (string key in (IEnumerable)x500Elements.Keys)
                this.SetParameter(key, (string)x500Elements[key]);
        }

        /// <summary>
        /// Return the final string version of the x500 Distinguished Name
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.DistinguishedName;
        }

        /// <summary>
        /// Set an element to an explicit binary value using hex encoding
        /// </summary>
        /// <param name="sElementName">name of element to set</param>
        /// <param name="byValue">byte array of the value to be encoded</param>
        public void SetBinaryValue(string sElementName, byte[] byValue)
        {
            this.SetParameter(sElementName, X500Name.HexEncode(byValue));
        }

        /// <summary>Get an element value as a binary array</summary>
        /// <param name="sElementName">name of element to retrieve</param>
        /// <returns>empty binary array if value not set, else binary array of bytes</returns>
        public byte[] GetBinaryValue(string sElementName)
        {
            return this.GetBinaryParameter(sElementName);
        }

        /// <summary>find the index of a parameter name in the list</summary>
        private int FindParameter(string sParam)
        {
            string str = sParam.ToLower() + "=";
            for (int index = 0; index < this._elements.Count; ++index)
            {
                if (((string)this._elements[index]).Length >= str.Length && ((string)this._elements[index]).Substring(0, str.Length).ToLower() == str)
                    return index;
            }
            return -1;
        }

        /// <summary>
        /// given an X.500 Distinguished Name, grab one of the component items (uid or ou, for example)
        /// </summary>
        /// <param name="sParam">DN element to retrieve (ex. "ou", "uid", etc.)</param>
        /// <returns>empty string if element not found, or string value of the element if found</returns>
        private string GetParameter(string sParam)
        {
            int parameter = this.FindParameter(sParam);
            if (parameter < 0)
                return "";
            string s = ((string)this._elements[parameter]).Substring(sParam.Length + 1);
            if (s.Length == 0)
                return "";
            if ((int)s[0] == 35)
                return X500Name.ParseHexEncodedStringAsString(ref s);
            return s;
        }

        /// <summary>
        /// given an X.500 Distinguished Name, grab one of the component items (uid or ou, for example) as a binary array of bytes
        /// </summary>
        /// <param name="sParam">DN element to retrieve (ex. "ou", "uid", etc.)</param>
        /// <returns>empty array if element not found, or byte[] value of the element if found</returns>
        private byte[] GetBinaryParameter(string sParam)
        {
            int parameter = this.FindParameter(sParam);
            if (parameter < 0)
                return new byte[0];
            string s = ((string)this._elements[parameter]).Substring(sParam.Length + 1);
            if (s.Length == 0)
                return new byte[0];
            if ((int)s[0] == 35)
                return X500Name.ParseHexEncodedStringAsByteArray(ref s);
            return new ASCIIEncoding().GetBytes(s);
        }

        /// <summary>
        /// set an x.500 parameter to a string value, encoding the value as necessary
        /// </summary>
        private void SetParameter(string sParam, string sValue)
        {
            int parameter = this.FindParameter(sParam);
            string str = sValue;
            if (X500Name.ContainsSpecialChars(sValue))
                str = X500Name.HexEncode(sValue);
            if (parameter < 0)
                this._elements.Add((sParam + "=" + str));
            else
                this._elements[parameter] = (((string)this._elements[parameter]).Substring(0, sParam.Length + 1) + sValue);
        }

        /// <summary>
        /// does the string contain any special x.500 chars that must be escaped?
        /// </summary>
        private static bool ContainsSpecialChars(string s)
        {
            return s.IndexOfAny(X500Name.EscapedSpecialChars) >= 0;
        }

        /// <summary>x.500 hex encode a byte array into a string</summary>
        private static string HexEncode(byte[] arr)
        {
            if (arr == null)
                throw new ArgumentNullException("arr");
            if (arr.Length == 0)
                return "";
            string str = "#";
            for (int index = 0; index < arr.Length; ++index)
                str += arr[index].ToString("x");
            return str;
        }

        /// <summary>x.500 hex encode a string into a string</summary>
        private static string HexEncode(string s)
        {
            if (s == null)
                throw new ArgumentNullException("s");
            if (string.IsNullOrEmpty(s))
                return "";
            return X500Name.HexEncode(new ASCIIEncoding().GetBytes(s));
        }

        /// <summary>
        /// handle the RFC 1779 special (escaped) pair case, i.e. "unescape" the string
        /// </summary>
        /// <param name="s">string to unescape</param>
        /// <returns>unescaped version of the string</returns>
        private static string UnEscapeString(string s)
        {
            string str = s;
            foreach (char escapedSpecialChar in X500Name.EscapedSpecialChars)
                str = str.Replace("\\" + escapedSpecialChar.ToString(), escapedSpecialChar.ToString());
            return str.Replace("\\\\", "\\");
        }

        /// <summary>parse a hex-encoded string and return it as a string</summary>
        private static string ParseHexEncodedStringAsString(ref string s)
        {
            if ((int)s[0] != 35)
                throw new ArgumentException("string not hex encoded", "s");
            return new ASCIIEncoding().GetString(X500Name.ParseHexEncodedStringAsByteArray(ref s));
        }

        /// <summary>
        /// parse a hex-encoded string and return it as a binary byte array
        /// </summary>
        private static byte[] ParseHexEncodedStringAsByteArray(ref string s)
        {
            if ((int)s[0] != 35)
                throw new ArgumentException("string not hex encoded", "s");
            s = s.Substring(1);
            int num = s.IndexOfAny(X500Name.ElementNameAndValueDelimiters);
            if (num < 0)
                num = s.Length;
            string HexString = s.Substring(0, num);
            s = num >= s.Length ? "" : s.Substring(num);
            return X500Name.ToByteArray(HexString);
        }

        /// <summary>convert a hex string to a byte array</summary>
        private static byte[] ToByteArray(string HexString)
        {
            int length = HexString.Length;
            byte[] numArray = new byte[length / 2];
            int startIndex = 0;
            while (startIndex < length)
            {
                numArray[startIndex / 2] = Convert.ToByte(HexString.Substring(startIndex, 2), 16);
                startIndex += 2;
            }
            return numArray;
        }

        /// <summary>
        /// parse out an element name or value, unescaping/ hex unencoding as necessary
        /// </summary>
        private string ParseElementString(ref string s)
        {
            string s1 = "";
            s = s.TrimStart(X500Name.WhiteSpace);
            if (s.Length == 0)
                return "";
            if ((int)s[0] == 35)
                return X500Name.ParseHexEncodedStringAsString(ref s);
            if ((int)s[0] == 34)
            {
                s = s.Substring(1);
                for (int index = 0; index < s.Length; ++index)
                {
                    if ((int)s[index] == 34)
                    {
                        if (index == 0 || (int)s[index - 1] != 92)
                        {
                            s = s.Substring(index + 1);
                            break;
                        }
                        s1 += s[index];
                    }
                    else
                        s1 += s[index];
                }
            }
            else
            {
                for (int startIndex = 0; startIndex < s.Length; ++startIndex)
                {
                    if (s[startIndex].ToString().IndexOfAny(X500Name.ElementNameAndValueDelimiters) >= 0)
                    {
                        if (startIndex == 0 || (int)s[startIndex - 1] != 92)
                        {
                            s = s.Substring(startIndex);
                            break;
                        }
                        s1 += s[startIndex];
                    }
                    else
                        s1 += s[startIndex];
                }
                if (s1 == s)
                    s = "";
                s1 = s1.TrimEnd(X500Name.WhiteSpace);
                s = s.TrimEnd(X500Name.WhiteSpace);
            }
            return X500Name.UnEscapeString(s1);
        }
    }
}
