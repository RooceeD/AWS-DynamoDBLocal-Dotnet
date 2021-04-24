using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace DynamoDbLocal.Tests
{
    public class DynamoDBLocalWrapperTest
    {
        [Fact]
        public async Task CreateTable_And_PutOneItem_In_TheTable_Using_DynamoDBClient()
        {
            using var dynamoDbProcess = DynamoDBLocalWrapper.CreateInMemoryDbProcess();
            dynamoDbProcess.Start();

            var dynamoDbClient = DynamoDBLocalWrapper.CreateClient();

            var createTableResponse = await dynamoDbClient.CreateTableAsync(new CreateTableRequest
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

            Assert.Equal(HttpStatusCode.OK, createTableResponse.HttpStatusCode);

            var putItemResponse = await dynamoDbClient.PutItemAsync(new PutItemRequest
            {
                TableName = "Example",
                Item = GetTestItem()
            });

            Assert.Equal(HttpStatusCode.OK, putItemResponse.HttpStatusCode);

            var scanResponse = await dynamoDbClient.ScanAsync(new ScanRequest
            {
                TableName = "Example"
            });

            Assert.Equal(HttpStatusCode.OK, scanResponse.HttpStatusCode);

            dynamoDbProcess.Kill(true);
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
