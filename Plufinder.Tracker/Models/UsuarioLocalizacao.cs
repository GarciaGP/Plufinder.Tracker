using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Plufinder.Tracker.Models
{
    class UsuarioLocalizacao
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("id_usuario")]
        public int IdUsuario { get; set; }

        [BsonElement("id_localizacao")]
        public int IdLocalizacao { get; set; }

        [BsonElement("id_cargo")]
        public int IdCargo { get; set; }
    }
}
