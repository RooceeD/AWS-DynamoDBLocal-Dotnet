using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;

namespace DynamoDbLocal.Helpers
{
    public abstract class DynamoDbLocalInMemoryBaseFixture : IDisposable
    {
        public Process? Process { get; private set; } = null;

        public AmazonDynamoDBClient? DynamoDbClient { get; private set; } = null;

        private int _port = 8000;

        public void Create(int port = 8000, string? environmentVariableName = "DYNAMODB_LOCAL_LATEST_COMMON_PATH", bool force = false)
        {
            if (Process == null || force) 
            {
                Dispose();

                this._port = port;

                Process = DynamoDbLocalWrapper.CreateInMemoryDbProcess(_port, environmentVariableName);
                Process.Start();

                DynamoDbClient = DynamoDbLocalWrapper.CreateClient(_port);
            }
        }

        public void Dispose()
        {
            Process?.Kill(true);
            
            DynamoDbClient?.Dispose();
        }

        public abstract Task SetupAsync();

        public abstract Task DestroyAsync();
    }
}
