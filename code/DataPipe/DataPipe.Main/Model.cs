﻿using Dapper.Contrib.Extensions;

namespace DataPipe.Main
{
    [Table("sync_entity")]
    public class sync_entity
    {[Key]
        public int entity_id { get; set; }
        public string entity_type { get; set; }
        public string unique_id { get; set; }
        public int dirty { get; set; }
        public int version { get; set; }        
    }
    [Table("sync_value_varchar")]
    public class sync_value_varchar
    {  [Key]
        public int id { get; set; }
        public int entity_id { get; set; }
        public string attribute_id { get; set; }
        public string value { get; set; }
        public int dirty { get; set; }
        public int version { get; set; }        
    }
}
