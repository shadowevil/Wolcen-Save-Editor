using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WolcenEditor
{
    public class SerializableExpandableObjectConverter : ExpandableObjectConverter
    {


        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return false;
            }
            else
            {
                return base.CanConvertTo(context, destinationType);
            }
        }
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return false;
            }
            else
            {
                return base.CanConvertFrom(context, sourceType);

            }
        }
    }
   
}
