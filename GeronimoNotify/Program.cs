using HtmlAgilityPack;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;

namespace GeronimoNotify
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            IConfiguration config = builder.Build();
            string username = config["Credentials:Username"];
            string password = config["Credentials:Password"];
            var addresses = config.GetSection("Addresses").GetChildren().Select(x => x.Value).ToArray();

            var client = new HttpClient();

            try
            {
                //učitaj prethodno dohvaćene izlete
                var stariIzleti = new List<Izlet>();
                if (File.Exists("izleti.json"))
                    stariIzleti = JsonSerializer.Deserialize<List<Izlet>>(File.ReadAllText("izleti.json"));
                
                //dohvat i parsing trenutnih izleta
                string content = await GetPageContent(client, username, password);
                List<Izlet> izleti = GetIzletiFromContent(content);

                //nađi diff gdje su novi, te usporedba s custom da se i promjene prate
                var diff = izleti.Except(stariIzleti, new IzletEq());                

                foreach (var izlet in diff)
                {
                    //za svaki novi izlet, složi poruku i probaj poslati msg
                    var msg = $"Izlet {izlet.display_name}, od {izlet.starttime} do {izlet.endtime}, mjesta {izlet.preostalo}, link: {izlet.link}";

                    await TrySendMessage(msg, addresses);
                }


                File.WriteAllText("izleti.json", JsonSerializer.Serialize(izleti));
            }
            catch (Exception ex)
            {
                await TrySendMessage(ex.Message, addresses);
            }
        }

        /// <summary>
        /// Iz html page content, dohvati listu izleta
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private static List<Izlet> GetIzletiFromContent(string content)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(content);

            //excursion-new je jedna od klasa div-a sa novim izletima
            var childNodes = doc.DocumentNode.Descendants(0)
                .Where(n => n.HasClass("excursion-new"));

            var izleti = new List<Izlet>();
            foreach (var node in childNodes)
            {
                //data marker conveniently sadrži json serijalizirane klase
                var data = node.Attributes.AttributesWithName("data-marker").First().Value;
                izleti.Add(JsonSerializer.Deserialize<Izlet>(data));
            }

            return izleti;
        }

        /// <summary>
        /// Dohvati html content stranice sa izletima
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private static async Task<string> GetPageContent(HttpClient client, string username, string password)
        {
            var authenticationString = $"{username}:{password}";
            var base64String = Convert.ToBase64String(
               System.Text.Encoding.UTF8.GetBytes(authenticationString));

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64String);

            var result = await client.GetAsync("https://rkgeronimo.hr");
            var content = await result.Content.ReadAsStringAsync();

            return content;
        }

        private static HttpClient ntfyClient = new HttpClient();

        /// <summary>
        /// Probaj poslati poruku, dump error in text ako ne
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private static async Task TrySendMessage(string msg, string[] addresses)
        {
            try
            {
                foreach (var adr in addresses)
                {                    
                    await ntfyClient.PostAsync(adr, new StringContent(msg));
                }                
            }
            catch (Exception ex) 
            {
                File.AppendAllText("error.txt", ex.Message);                
            }
        }
    }
    
}


public class Izlet
{
    public string id { get; set; }
    public string latitude { get; set; }
    public string longitude { get; set; }
    public DateOnly starttime { get; set; }
    public DateOnly endtime { get; set; }
    public DateOnly deadline { get; set; }
    public string limitation { get; set; }
    public string registered { get; set; }
    public string post_title { get; set; }
    public string display_name { get; set; }
    public string canceled { get; set; }
    public string link { get; set; }

    [JsonIgnore]
    public int preostalo 
    { 
        get 
        {
            int r = Convert.ToInt32(limitation) - Convert.ToInt32(registered); 

            return r > 0 ? r : 0;
        } 
    }
}

public class IzletEq : IEqualityComparer<Izlet>
{
    public bool Equals(Izlet? x, Izlet? y)
    {
        if (x == null && y == null)
            return true;
        if (x == null || y == null)
            return false;

        return x.id == y.id
            && x.starttime == y.starttime
            && x.endtime == y.endtime
            && x.limitation == y.limitation
            //ako se broj registriranih smanjio
            && Convert.ToInt32(x.registered) <= Convert.ToInt32(y.registered);
    }

    public int GetHashCode([DisallowNull] Izlet obj)
    {
        return obj.id.GetHashCode();
    }
}
