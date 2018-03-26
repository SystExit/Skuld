using System;
using System.Threading.Tasks;
using Skuld.Models.API;

namespace Skuld.APIS.Animals
{
    public class RandomDog
    {
        public static async Task<string> GetDoggoAsync()
        {
			var resp = await WebHandler.ReturnStringAsync(new Uri("https://random.dog/woof"));
			if (resp == null) return null;
			return "https://random.dog/" + resp;
        }
    }
}
