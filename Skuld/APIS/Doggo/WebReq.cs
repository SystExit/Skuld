using System;
using System.Threading.Tasks;
using Skuld.Models.API;

namespace Skuld.APIS.Doggo
{
    public class WebReq
    {
        public static async Task<DoggoImage> GetDoggo()
        {
            return new DoggoImage(await APIWebReq.ReturnString(new Uri("http://random.dog/woof")));
        }
    }
}
