using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rs3Tracker {
    public class Keypressed : KeybindClass {
        public double timepressed { get; set; }
    }


    public class BarKeybindClass : BarClass {
        public string modifier { get; set; }
        public string key { get; set; }
        public BarClass bar { get; set; }
    }

    public class BarClass {
        public string name { get; set; }
    }

    public class Ability {
        public string name { get; set; }
        public double cooldown { get; set; }
        public string img { get; set; }
    }

    public class KeybindClass {
        public string modifier { get; set; }
        public string key { get; set; }
        public BarClass bar { get; set; }
        public Ability ability { get; set; }
    }
}
