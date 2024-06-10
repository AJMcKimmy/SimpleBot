using System.IO;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
public class JSONReader
	{
		public string token { get; set; }

		public async Task ReadJSON() 
		{
			using (StreamReader sr = new StreamReader("config/token.json", new UTF8Encoding(false)))
			{
				string json = await sr.ReadToEndAsync();
				ConfigJSON obj = JsonConvert.DeserializeObject<ConfigJSON>(json); 
				
				this.token = obj.Token; 
			}
		}

		public Task WriteJSON(string token)
		{
			JObject tokenField = new JObject(
				new JProperty("token", token)
			);
			File.WriteAllText("config/token.json", tokenField.ToString());
			using(StreamWriter file = File.CreateText("config/token.json"))
			using(JsonTextWriter writer = new JsonTextWriter(file))
			{
				tokenField.WriteTo(writer);
			}
			return Task.CompletedTask;
		}

		internal sealed class ConfigJSON
		{
			public string Token { get; set; }
		}
	}