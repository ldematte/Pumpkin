﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pumpkin;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace PumpkinTests {
    [TestClass]
    public class CompilationTests {
        [TestMethod]
        public void CSharpProviderAssembliesShouldBeTransparent() {

            var snippetSource = File.ReadAllText(@"..\..\Tests\FileRead.cs");
            var snippetAssembly = Pumpkin.SnippetCompiler.CompileWithCSC(snippetSource);

            var assembly = System.Reflection.Assembly.Load(snippetAssembly.Item1);
            Assert.IsTrue(assembly.GetType("Snippets.FileRead").GetMethod("SnippetMain").IsSecurityTransparent);
        }

        [TestMethod]
        public void CecilWhitelistAssembly() {
            var snippetSource = File.ReadAllText(@"..\..\Tests\CreateObjectWithActivator.cs");
            var snippetAssembly = Pumpkin.SnippetCompiler.CompileWithCSC(snippetSource);

            var whiteList = new List<ListEntry> { new ListEntry("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089") };

            Assert.IsTrue(SnippetCompiler.CheckAssemblyAgainstWhitelist(snippetAssembly.Item1, whiteList));
        }

        [TestMethod]
        public void CecilWhitelistTypes() {
            var snippetSource = File.ReadAllText(@"..\..\Tests\CreateObjectWithActivator.cs");
            var snippetAssembly = Pumpkin.SnippetCompiler.CompileWithCSC(snippetSource);

            var whiteList = new List<ListEntry> { 
                new ListEntry("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", "System.Type"),
                new ListEntry("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", "System.Object"),
                new ListEntry("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", "System.Threading.Thread"),
                new ListEntry("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", "System.Activator")
            };

            Assert.IsTrue(SnippetCompiler.CheckAssemblyAgainstWhitelist(snippetAssembly.Item1, whiteList));
        }

        [TestMethod]
        public void CecilWhitelistMethods() {
            var snippetSource = File.ReadAllText(@"..\..\Tests\CreateObjectWithActivator.cs");
            var snippetAssembly = Pumpkin.SnippetCompiler.CompileWithCSC(snippetSource);

            var whiteList = new List<ListEntry> { 
                new ListEntry("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", "System.Type"),
                new ListEntry("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", "System.Object"),
                new ListEntry("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", "System.Threading.Thread", "Start"),
                new ListEntry("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", "System.Activator")
            };

            Assert.IsTrue(SnippetCompiler.CheckAssemblyAgainstWhitelist(snippetAssembly.Item1, whiteList));
        }

        [TestMethod]
        public void CecilWhitelistMethodsNotListed() {
            var snippetSource = File.ReadAllText(@"..\..\Tests\CreateObjectWithActivator.cs");
            var snippetAssembly = Pumpkin.SnippetCompiler.CompileWithCSC(snippetSource);

            var whiteList = new List<ListEntry> { 
                new ListEntry("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", "System.Type"),
                new ListEntry("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", "System.Object"),
                new ListEntry("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", "System.Threading.Thread", "Abort"),
                new ListEntry("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", "System.Activator")
            };

            Assert.IsFalse(SnippetCompiler.CheckAssemblyAgainstWhitelist(snippetAssembly.Item1, whiteList));
        }

        [TestMethod]
        public void CecilPatchStaticCall() {
            var snippetSource = File.ReadAllText(@"..\..\Tests\HelloWorld.cs");
            var snippetAssembly = Pumpkin.SnippetCompiler.CompileWithCSC(snippetSource);

            var patchedAssembly = SnippetCompiler.PatchAssembly(snippetAssembly.Item1, "Snippets.HelloWorld");

            var snippetResult = SnippetRunner.Run(patchedAssembly, "Snippets.HelloWorld");

            Assert.IsTrue(snippetResult.Success);
            Assert.IsNull(snippetResult.Exception);

            Assert.AreEqual("Hello world!", snippetResult.Output.FirstOrDefault());
        }

        [TestMethod]
        public void CecilPatchMethodCreation() {
            var snippetSource = File.ReadAllText(@"..\..\Tests\ObjectCreation.cs");
            var snippetAssembly = Pumpkin.SnippetCompiler.CompileWithCSC(snippetSource);

            var patchedAssembly = SnippetCompiler.PatchAssembly(snippetAssembly.Item1, "Snippets.ObjectCreation");            

            var snippetResult = SnippetRunner.Run(patchedAssembly, "Snippets.ObjectCreation");

            Assert.IsTrue(snippetResult.Success);
            Assert.IsNull(snippetResult.Exception);

            Assert.AreEqual("0 - Hello", snippetResult.Output.FirstOrDefault());
            Assert.AreEqual(10, snippetResult.Output.Count);
            Assert.AreEqual(10, snippetResult.Monitor.NumberOfObjectCreations);
        }
    }
}
