using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WolcenEditor
{
    ///Represents the CharacterCustomization object in the json file.
    public class CharacterCustomization
    {
        public int Sex { get; set; }
        public int Face { get; set; }
        public int SkinColor { get; set; }
        public int Haircut { get; set; }
        public int HairColor { get; set; }
        public int Beard { get; set; }
        public int BeardColor { get; set; }
        public int LeftEye { get; set; }
        public int RightEye { get; set; }
        public int Archetype { get; set; }
    }
}
