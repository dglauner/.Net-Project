using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCIE237LegacyComponent;

namespace E33DGLAUNER
{
    public class CodeObj
    {
        //The object used to store info for a ComboBox in a databound list
        private string m_Name;
        private short m_code;

        public CodeObj() { }

        public CodeObj(short inCode, string inName)
        {
            m_Name = inName;
            m_code = inCode;
        }

        public CodeObj(Class2 c)
        {
            m_Name = c.Name;
            m_code = c.Code;
        }

        public short Code
        {
            get
            {
                return this.m_code;
            }
            set
            {
                m_code = value;
            }
        }

        public string Name
        {
            get
            {
                return this.m_Name;
            }
            set
            {
                m_Name = value;
            }
        }

        public override string ToString()
        {
            return this.Name + " : " + this.Code;
        }
    }
}
