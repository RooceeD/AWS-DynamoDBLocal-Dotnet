# AWS DynamoDB Local For Testing in .NET Core

_It is just a wrapper for java version, provided by AWS._

## Setup

1. [Download DynamoDBLocal](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DynamoDBLocal.DownloadingAndRunning.html)
2. Put content of the folder in *ProjectRootDir/dynamodb_local_latest*
3. Usage:

``` csharp
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
```

## Troubleshooting

1. Make sure to always kill a running process at the end and first verify, on any error, there is no active dynamodb java processes (task manager)

``` csharp
dynamoDbProcess.Kill(true);
```

2. Make sure DynamoDBLocal and DynamoDBClient both are using the same port, otherwise the client may not be able to find the runnig java process.
[Read more...](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/CodeSamples.DotNet.html#CodeSamples.DotNet.RegionAndEndpoint)

3. Alternative usage (not tested, also make sure aws cli is configured):

```csharp
private void CreateTable_UsingCommand()
{
    var input = @"aws dynamodb create-table --table-name Example --attribute-definitions AttributeName=Id,AttributeType=S AttributeName=CreatedDate,AttributeType=S --key-schema AttributeName=Id,KeyType=HASH AttributeName=CreatedDate,KeyType=RANGE --provisioned-throughput ReadCapacityUnits=10,WriteCapacityUnits=5 --endpoint-url http://localhost:8000 --region test";
    using var createTableProcess = DynamoDBLocalWrapper.CreateProcess(input);
    createTableProcess.Start();
    createTableProcess.Kill(true);
}
```

4. SharedDb (not tested):
```csharp
private void CreateSharedDb()
{
    using var dynamoDbProcess = DynamoDBLocalWrapper.CreateSharedDbProcess();
    dynamoDbProcess.Start();

    var dynamoDbClient = DynamoDBLocalWrapper.CreateClient();

    // do something

    dynamoDbProcess.Kill(true);
}
```
