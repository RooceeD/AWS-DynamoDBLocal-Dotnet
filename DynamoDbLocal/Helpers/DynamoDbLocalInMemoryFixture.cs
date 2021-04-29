using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;

namespace DynamoDbLocal.Helpers
{
    public class DynamoDbLocalInMemoryFixture : DynamoDbLocalInMemoryBaseFixture
    {
        public override async Task SetupAsync()
        {
            await CreateExampleTableAsync();
        }

        public override async Task DestroyAsync()
        {
            await DeleteAllTablesAsync();
        }

        private async Task CreateExampleTableAsync() 
        {
            if (DynamoDbClient != null)
            {
                await DynamoDbClient.CreateTableAsync(new CreateTableRequest
                {
                    TableName = "Example",
                    AttributeDefinitions = new List<AttributeDefinition>
                    {
                        new AttributeDefinition
                        {
                            AttributeName = "Id",
                            AttributeType = "S"
                        },
                        new AttributeDefinition
                        {
                            AttributeName = "CreatedDate",
                            AttributeType = "S"
                        }
                    },
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement
                        {
                            AttributeName = "Id",
                            KeyType = "HASH"
                        },
                        new KeySchemaElement
                        {
                            AttributeName = "CreatedDate",
                            KeyType = "RANGE"
                        }
                    },
                    ProvisionedThroughput = new ProvisionedThroughput
                    {
                        ReadCapacityUnits = 10,
                        WriteCapacityUnits = 5
                    }
                });
            }
        }

        private async Task DeleteAllTablesAsync()
        {
            if (DynamoDbClient != null)
            {
                var tables = await DynamoDbClient.ListTablesAsync();
                foreach (var tableName in tables.TableNames)
                {
                    await DynamoDbClient.DeleteTableAsync(tableName);
                }
            }
        }
    }
}
