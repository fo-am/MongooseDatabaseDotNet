﻿
using System;

namespace DataReceiver.Main.Model.PupFocal
{
    public class PupFind : ISendable
    {
        public int sent { get; set; }
        public string UniqueId { get; set; }
        public int entity_id { get; set; }
        public string entity_type { get; set; }

        public string size { get; set; }
        public DateTime time { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }

    }
}
