﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Recipe11_2
{
    class Program
    {
        static void Main(string[] args)
        {
            var t = AsynchronousProcessing();
            t.GetAwaiter().GetResult();

            Console.WriteLine();
            Console.WriteLine("Press ENTER to exit.");
            Console.ReadLine();
        }

        async static Task AsynchronousProcessing()
        {
            StorageFolder folder = KnownFolders.DocumentsLibrary;

            if (await folder.DoesFileExistAsync("test.txt"))
            {
                var fileToDelete = await folder.GetFileAsync("test.txt");
                await fileToDelete.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }

            var file = await folder.CreateFileAsync("test.txt", CreationCollisionOption.ReplaceExisting);
            using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            using (var writer = new StreamWriter(stream.AsStreamForWrite()))
            {
                await writer.WriteLineAsync("Test content");
                await writer.FlushAsync();
            }
            
            using (var stream = await file.OpenAsync(FileAccessMode.Read))
            using (var reader = new StreamReader(stream.AsStreamForRead()))
            {
                string content = await reader.ReadToEndAsync();
                Console.WriteLine(content);
            }

            Console.WriteLine("Enumerating Folder Structure:");

            var itemsList = await folder.GetItemsAsync();
            foreach (var item in itemsList)
            {
                if (item is StorageFile)
                {
                    Console.WriteLine("{0} folder", item.Name);
                }
                else
                {
                    Console.WriteLine(item.Name);
                }
            }
        }
    }

    static class Extensions
    {
        public static async Task<bool> DoesFileExistAsync(this StorageFolder folder, string fileName)
        {
            try
            {
                await folder.GetFileAsync(fileName);
                return true;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }
    }
}
