using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using DynamoDbLocal.Helpers;
using Xunit;

namespace DynamoDbLocal.Tests
{
    public class DynamoDbLocalInMemoryFixtureTest: IClassFixture<DynamoDbLocalInMemoryFixture>
    {
        private const string TableName = "Example";
        private readonly DynamoDbLocalInMemoryFixture _dynamoDbLocalInMemoryFixture;

        public DynamoDbLocalInMemoryFixtureTest(DynamoDbLocalInMemoryFixture dynamoDbLocalInMemoryFixture)
        {
            _dynamoDbLocalInMemoryFixture = dynamoDbLocalInMemoryFixture;
            _dynamoDbLocalInMemoryFixture.Create();
        }

        [Fact]
        public async Task CreateTable_And_PutOneItem_In_TheTable_Using_DynamoDBClient()
        {
            await _dynamoDbLocalInMemoryFixture.SetupAsync();

            var dynamoDbClient = _dynamoDbLocalInMemoryFixture.DynamoDbClient;
            
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
            Assert.Equal(1, scanResponse.Items.Count);

            await _dynamoDbLocalInMemoryFixture.DestroyAsync();
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
