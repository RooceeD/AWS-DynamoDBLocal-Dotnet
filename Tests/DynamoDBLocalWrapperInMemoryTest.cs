using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace DynamoDbLocal.Tests
{
    public class DynamoDBLocalWrapperInMemoryTest
    {
        private const string TableName = "Example2";

        [Fact]
        public async Task CreateTable_And_PutOneItem_In_TheTable_Using_DynamoDBClient()
        {
            Exception exception = null;
            using var dynamoDbProcess = DynamoDbLocalWrapper.CreateInMemoryDbProcess(8001, "DYNAMODB_LOCAL_LATEST_COMMON_PATH");

            try
            {
                dynamoDbProcess.Start();

                using var dynamoDbClient = DynamoDbLocalWrapper.CreateClient(8001);

                var createTableResponse = await dynamoDbClient.CreateTableAsync(new CreateTableRequest
                {
                    TableName = TableName,
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

                Assert.Equal(HttpStatusCode.OK, createTableResponse.HttpStatusCode);

                var putItemResponse = await dynamoDbClient.PutItemAsync(new PutItemRequest
                {
                    TableName = TableName,
                    Item = GetTestItem()
                });

                Assert.Equal(HttpStatusCode.OK, putItemResponse.HttpStatusCode);

                var scanResponse = await dynamoDbClient.ScanAsync(new ScanRequest
                {
                    TableName = TableName
                });

                Assert.Equal(HttpStatusCode.OK, scanResponse.HttpStatusCode);
            } catch (Exception ex) {
                exception = ex;
            }

            dynamoDbProcess.Kill(true);

            if (exception != null)
            {
                throw exception;
            }
        }

        private Dictionary<string, AttributeValue> GetTestItem()
        {
            Dictionary<string, AttributeValue> attributes = new ();
            attributes["Id"] = new AttributeValue { S = Guid.NewGuid().ToString() };
            attributes["CreatedDate"] = new AttributeValue { S = DateTimeOffset.UtcNow.ToString() };
            attributes["Content"] = new AttributeValue { S = "aws_dynamodb_local_test" };
            return attributes;
        }
    }
}
