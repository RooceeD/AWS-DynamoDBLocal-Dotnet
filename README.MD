# AWS DynamoDB Local For Testing in .NET Core

_It is just a wrapper for java version, provided by AWS._

## Setup

1. [Download and configure AWS CLI V2](https://docs.aws.amazon.com/cli/latest/userguide/install-cliv2.html)
2. [Download DynamoDBLocal](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DynamoDBLocal.DownloadingAndRunning.html)
3. Put content of the folder in *SolutionRootDir/dynamodb_local_latest* or use environment variable, for example **DYNAMODB_LOCAL_LATEST_COMMON_PATH**

## Usage

1. Example (also see **DynamoDbLocalInMemoryBaseFixture** and **DynamoDbLocalInMemoryFixture**)

    ``` csharp
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
    ```

2. Make sure to always kill a running process at the end and first verify, on any error, there is no active dynamodb java processes (task manager)

    ``` csharp
    dynamoDbProcess.Kill(true);
    ```

3. Make sure DynamoDBLocal and DynamoDBClient both are using the same port, otherwise the client may not be able to find the runnig java process.
[Read more...](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/CodeSamples.DotNet.html#CodeSamples.DotNet.RegionAndEndpoint)

4. Alternative usage:

    ```csharp
    private void CreateTable_UsingCommand()
    {
        // create using environment variable
        using var dynamoDbProcess = DynamoDbLocalWrapper.CreateInMemoryDbProcess(environmentVariableName: "DYNAMODB_LOCAL_LATEST_COMMON_PATH");

        // or put dynamodb_local_latest folder inside the solution root folder
        // using var dynamoDbProcess = DynamoDbLocalWrapper.CreateInMemoryDbProcess();

        dynamoDbProcess.Start();

        var input = @"aws dynamodb create-table --table-name Example --attribute-definitions AttributeName=Id,AttributeType=S AttributeName=CreatedDate,AttributeType=S --key-schema AttributeName=Id,KeyType=HASH AttributeName=CreatedDate,KeyType=RANGE --provisioned-throughput ReadCapacityUnits=10,WriteCapacityUnits=5 --endpoint-url http://localhost:8000 --region test";
        using var createTableProcess = DynamoDBLocalWrapper.CreateProcess(input);
        createTableProcess.Start();
        createTableProcess.Kill(true);

        // do something else

        dynamoDbProcess.Kill(true);
    }
    ```

5. SharedDb:

    ```csharp
    private void CreateSharedDb()
    {
        // create using environment variable
        using var dynamoDbProcess = DynamoDbLocalWrapper.CreateSharedDbProcess(environmentVariableName: "DYNAMODB_LOCAL_LATEST_COMMON_PATH");

        // or put dynamodb_local_latest folder inside the solution root folder
        // using var dynamoDbProcess = DynamoDbLocalWrapper.CreateSharedDbProcess();

        dynamoDbProcess.Start();

        using var dynamoDbClient = DynamoDbLocalWrapper.CreateClient();

        // do something

        dynamoDbProcess.Kill(true);
    }
    ```
