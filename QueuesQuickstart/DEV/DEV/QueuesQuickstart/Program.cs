using Azure;
using Azure.Identity;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using System;
using System.Threading.Tasks;

/// <summary>
/// Master: Sample console application that demonstrates basic operations with Azure Queue Storage using the Azure.Storage.Queues SDK.
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("Azure Queue Storage client library - .NET quickstart sample");

            string queueName = "lmdqueu";
            string storageAccountName = "lmdstorageaccount";

            QueueClient queueClient = new QueueClient(
                new Uri($"https://{storageAccountName}.queue.core.windows.net/{queueName}"),
                new DefaultAzureCredential());

            Console.WriteLine($"Creating queue: {queueName}");

            await queueClient.CreateIfNotExistsAsync();

            Console.WriteLine("\nAdding messages to the queue...");

            await queueClient.SendMessageAsync("First message");
            await queueClient.SendMessageAsync("Second message");

            Response<SendReceipt> response = await queueClient.SendMessageAsync("Third message");
            SendReceipt receipt = response.Value;

            Console.WriteLine("\nPeek at the messages in the queue...");

            PeekedMessage[] peekedMessages = await queueClient.PeekMessagesAsync(maxMessages: 10);

            foreach (PeekedMessage peekedMessage in peekedMessages)
            {
                Console.WriteLine($"Message: {peekedMessage.MessageText}");
            }

            Console.WriteLine("\nUpdating the third message in the queue...");

            await queueClient.UpdateMessageAsync(receipt.MessageId, receipt.PopReceipt, "Third message has been updated");

            QueueProperties properties = queueClient.GetProperties();
            int cachedMessagesCount = properties.ApproximateMessagesCount;

            Console.WriteLine($"Number of messages in queue: {cachedMessagesCount}");

            Console.WriteLine("\nReceiving messages from the queue...");

            QueueMessage[] messages = await queueClient.ReceiveMessagesAsync(maxMessages: 10);

            Console.WriteLine("\nPress Enter key to 'process' messages and delete them from the queue...");
            Console.ReadLine();

            foreach (QueueMessage message in messages)
            {
                Console.WriteLine($"Message: {message.MessageText}");
                await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);
            }

            Console.WriteLine("\nPress Enter key to delete the queue...");
            Console.ReadLine();

            Console.WriteLine($"Deleting queue: {queueClient.Name}");
            await queueClient.DeleteAsync();

            Console.WriteLine("Done");
        }
        catch (RequestFailedException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }
    }
}
