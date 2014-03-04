﻿using System;
using System.IO;
using Altairis.BinaryStore.FileSystem;
using Altairis.BinaryStore.WindowsAzure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Altairis.BinaryStore.Tests
{
    [TestClass]
    public class IntegrationTests
    {

        public TestContext TestContext { get; set; }

        private string TestFilePath {
            get { return Path.Combine(TestContext.TestRunDirectory, "..\\..\\Tests\\Altairis.BinaryStore.Tests\\test.txt"); }
        }


        [TestMethod]
        public void FileSystem_ComplexTest() {
            var provider = new FileSystemStoreProvider() {
                FolderName = Path.Combine(TestContext.TestRunDirectory, "FileSystemStore")
            };

            DoTest(provider);
        }

        [TestMethod]
        public void Blob_ComplexTest() {
            var provider = new BlobStoreProvider() {
                ConnectionStringName = "DevStorageAccount"
            };

            DoTest(provider);
        }

        private void DoTest(StoreProvider provider) {
            // save bytes as new file
            provider.Save("test.txt", File.ReadAllBytes(TestFilePath));

            // save stream as new file
            using (var fs = new FileStream(TestFilePath, FileMode.Open, FileAccess.Read)) {
                provider.Save("folder/folder2/test2.txt", fs);
            }

            // file exists
            Assert.IsTrue(provider.Exists("test.txt"));
            Assert.IsTrue(provider.Exists("folder/folder2/test2.txt"));
            Assert.IsFalse(provider.Exists("non-existing-file.txt"));

            byte[] bytes = null;
            Stream s = null;
            try {
                // load bytes
                Assert.IsTrue(provider.Load("test.txt", out bytes));

                // load stream
                Assert.IsTrue(provider.Load("folder/folder2/test2.txt", out s));

                // compare
                for (var i = 0; i < bytes.Length; i++) {
                    Assert.AreEqual(s.ReadByte(), bytes[i]);
                }
                Assert.AreEqual(s.Position, s.Length);
            }
            finally {
                s.Dispose();
            }

            // overwrite file and read it to detect whether the changes were really saved
            var newData = new byte[] { 1, 2, 3, 4, 5 };
            provider.Save("test.txt", newData);
            byte[] newData2;
            provider.Load("test.txt", out newData2);
            for (int i = 0; i < newData.Length; i++)
            {
                Assert.AreEqual(newData[i], newData2[i]);
            }
            Assert.AreEqual(newData.Length, newData2.Length);

            // delete files
            provider.Delete("test.txt");
            provider.Delete("folder/folder2/test2.txt");

            // file exists
            Assert.IsFalse(provider.Exists("test.txt"));
            Assert.IsFalse(provider.Exists("folder/folder2/test2.txt"));
        }
    }

}
