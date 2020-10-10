using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vergiBlue.Connection
{
    class ClientImplementation
    {
        readonly MovementStream.MovementStreamClient client;

        public ClientImplementation(MovementStream.MovementStreamClient client)
        {
            this.client = client;
        }

        public async Task<GameStartInformation> Initialize(string clientName)
        {
            var information = new ClientInformation()
            {
                Name = clientName
            };

            Console.WriteLine("Initializing client... Getting start information from server.");
            var startInformation = await client.InitializeAsync(information);

            Console.WriteLine($"Received info: start player: {startInformation.WhitePlayer}.");
            return startInformation;

        }
    }
}
