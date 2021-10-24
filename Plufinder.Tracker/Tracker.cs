using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MongoDB.Driver;
using Plufinder.Tracker.Models;
using MongoDB.Bson;

namespace Plufinder.Tracker
{
    public static class Tracker
    {
        [FunctionName("PlufinderTracker")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)

        {
            string responseMessage = "mongodb+srv://plufinderatlas:fiap123@plufindertracker.l6ath.mongodb.net/Plufinder?connect=replicaSet";
            log.LogInformation("Inicializando Azure Function . . .");
            int usuario = int.Parse(req.Query["usuario"]);
            int cargo = int.Parse(req.Query["cargo"]);
            int localizacao = int.Parse(req.Query["localizacao"]);

            log.LogInformation("Realizando conex�o com o MongoDB . . .");
            var client = new MongoClient("mongodb+srv://plufinderatlas:fiap123@plufindertracker.l6ath.mongodb.net/Plufinder?connect=replicaSet");
            var session = client.StartSession();
            var database = client.GetDatabase("Plufinder");
            var collection = database.GetCollection<UsuarioLocalizacao>("UserLocation");

            try
            {
                var filter = new FilterDefinitionBuilder<UsuarioLocalizacao>().Where(u => u.IdUsuario == usuario);
                //var update = Builders<UsuarioLocalizacao>.Update.Set

                log.LogInformation("Verificando se o usu�rio j� est� na base de localiza��o . . .");
                var usuarioBase = await collection.Find(Builders<UsuarioLocalizacao>.Filter.Eq("id_usuario", usuario)).FirstOrDefaultAsync();

                session.StartTransaction();
                if (usuarioBase != null)
                {
                    usuarioBase.IdLocalizacao = localizacao;
                    log.LogInformation("Atualizando a localiza��o do usu�rio . . .");
                    await collection.FindOneAndReplaceAsync(filter, usuarioBase);
                }
                else
                {
                    log.LogInformation("Inserindo novo usu�rio nas localiza��es . . .");
                    var novoUsuarioLocalizacao = new UsuarioLocalizacao
                    {
                        Id = new ObjectId(),
                        IdUsuario = usuario,
                        IdLocalizacao = localizacao,
                        IdCargo = cargo
                       
                    };
                    await collection.InsertOneAsync(novoUsuarioLocalizacao);
                }

                responseMessage = $"Opera��o conclu�da - Usu�rio {usuario}, de cargo {cargo}, est� na localiza��o {localizacao}";
                log.LogInformation("Opera��o conclu�da sem exce��es - realizando commit . . .");
                session.CommitTransaction();
            }
            catch (Exception e)
            {
                session.AbortTransaction();
                return new OkObjectResult("Erro: " + e.Message);
                throw;
            }

            return new JsonResult(responseMessage);
        }
    }
}
