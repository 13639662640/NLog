// 
// Copyright (c) 2004-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 

using System;
using System.Xml;
using System.Globalization;

using NLog;
using NLog.Config;

using NUnit.Framework;
using NLog.LayoutRenderers;

namespace NLog.UnitTests
{
    [TestFixture]
	public class LayoutConfigurationTests : NLogTestBase
	{
        [Test]
        public void SimpleTest()
        {
            Layout l = new Layout("${message}");
            Assert.AreEqual(1, l.Renderers.Length);
            Assert.IsInstanceOfType(typeof(NLog.LayoutRenderers.MessageLayoutRenderer), l.Renderers[0]);
        }

        [Test]
        public void UnclosedTest()
        {
            Layout l = new Layout("${message");
        }

        [Test]
        public void SingleParamTest()
        {
            Layout l = new Layout("${mdc:item=AAA}");
            Assert.AreEqual(1, l.Renderers.Length);
            MDCLayoutRenderer mdc = l.Renderers[0] as MDCLayoutRenderer;
            Assert.IsNotNull(mdc);
            Assert.AreEqual("AAA", mdc.Item);
        }

        [Test]
        public void ValueWithColonTest()
        {
            Layout l = new Layout("${mdc:item=AAA\\:}");
            Assert.AreEqual(1, l.Renderers.Length);
            MDCLayoutRenderer mdc = l.Renderers[0] as MDCLayoutRenderer;
            Assert.IsNotNull(mdc);
            Assert.AreEqual("AAA:", mdc.Item);
        }

        [Test]
        public void ValueWithBracketTest()
        {
            Layout l = new Layout("${mdc:item=AAA\\}\\:}");
            Assert.AreEqual("${mdc:item=AAA\\}\\:}", l.Text);
            Assert.AreEqual(1, l.Renderers.Length);
            MDCLayoutRenderer mdc = l.Renderers[0] as MDCLayoutRenderer;
            Assert.IsNotNull(mdc);
            Assert.AreEqual("AAA}:", mdc.Item);
        }

        [Test]
        public void DefaultValueTest()
        {
            Layout l = new Layout("${mdc:BBB}");
            Assert.AreEqual(1, l.Renderers.Length);
            MDCLayoutRenderer mdc = l.Renderers[0] as MDCLayoutRenderer;
            Assert.IsNotNull(mdc);
            Assert.AreEqual("BBB", mdc.Item);
        }

        [Test]
        public void DefaultValueWithBracketTest()
        {
            Layout l = new Layout("${mdc:AAA\\}\\:}");
            Assert.AreEqual(l.Text,"${mdc:AAA\\}\\:}");
            Assert.AreEqual(1, l.Renderers.Length);
            MDCLayoutRenderer mdc = l.Renderers[0] as MDCLayoutRenderer;
            Assert.IsNotNull(mdc);
            Assert.AreEqual("AAA}:", mdc.Item);
        }

        [Test]
        public void DefaultValueWithOtherParametersTest()
        {
            Layout l = new Layout("${mdc:BBB:padding=3:padcharacter=X}");
            Assert.AreEqual(1, l.Renderers.Length);
            MDCLayoutRenderer mdc = l.Renderers[0] as MDCLayoutRenderer;
            Assert.IsNotNull(mdc);
            Assert.AreEqual("BBB", mdc.Item);
            Assert.AreEqual(3, mdc.Padding);
            Assert.AreEqual('X', mdc.PadCharacter);
        }

        [Test]
        public void EmptyValueTest()
        {
            Layout l = new Layout("${mdc:item=}");
            Assert.AreEqual(1, l.Renderers.Length);
            MDCLayoutRenderer mdc = l.Renderers[0] as MDCLayoutRenderer;
            Assert.IsNotNull(mdc);
            Assert.AreEqual("", mdc.Item);
        }

        [Test]
        public void NestedLayoutTest()
        {
            Layout l = new Layout("${file-contents:fileName=${basedir:padding=10}/aaa.txt:padding=12}");
            Assert.AreEqual(1, l.Renderers.Length);
            FileContentsLayoutRenderer lr = l.Renderers[0] as FileContentsLayoutRenderer;
            Assert.IsNotNull(lr);
            Assert.IsInstanceOfType(typeof(Layout), lr.FileName);
            Assert.AreEqual("${basedir:padding=10}/aaa.txt", lr.FileName.Text);
            Assert.AreEqual(1, lr.FileName.Renderers.Length);
            Assert.AreEqual(12, lr.Padding);
        }

        [Test]
        public void DoubleNestedLayoutTest()
        {
            Layout l = new Layout("${file-contents:fileName=${basedir}/${file-contents:fileName=${basedir}/aaa.txt}/aaa.txt}");
            Assert.AreEqual(1, l.Renderers.Length);
            FileContentsLayoutRenderer lr = l.Renderers[0] as FileContentsLayoutRenderer;
            Assert.IsNotNull(lr);
            Assert.IsInstanceOfType(typeof(Layout), lr.FileName);
            Assert.AreEqual("${basedir}/${file-contents:fileName=${basedir}/aaa.txt}/aaa.txt", lr.FileName.Text);
            Assert.AreEqual(3, lr.FileName.Renderers.Length);
            Assert.IsInstanceOfType(typeof(LiteralLayoutRenderer), lr.FileName.Renderers[0]);
            Assert.IsInstanceOfType(typeof(FileContentsLayoutRenderer), lr.FileName.Renderers[1]);
            Assert.IsInstanceOfType(typeof(LiteralLayoutRenderer), lr.FileName.Renderers[2]);

            LiteralLayoutRenderer lr1 = (LiteralLayoutRenderer)lr.FileName.Renderers[0];
            FileContentsLayoutRenderer fc = (FileContentsLayoutRenderer)lr.FileName.Renderers[1];
            LiteralLayoutRenderer lr2 = (LiteralLayoutRenderer)lr.FileName.Renderers[2];

            Assert.AreEqual("${basedir}/aaa.txt", fc.FileName.Text);

        }

        [Test]
        public void DoubleNestedLayoutWithDefaultLayoutParametersTest()
        {
            Layout l = new Layout("${file-contents:${basedir}/${file-contents:${basedir}/aaa.txt}/aaa.txt}");
            Assert.AreEqual(1, l.Renderers.Length);
            FileContentsLayoutRenderer lr = l.Renderers[0] as FileContentsLayoutRenderer;
            Assert.IsNotNull(lr);
            Assert.IsInstanceOfType(typeof(Layout), lr.FileName);
            Assert.AreEqual("${basedir}/${file-contents:${basedir}/aaa.txt}/aaa.txt", lr.FileName.Text);
            Assert.AreEqual(3, lr.FileName.Renderers.Length);
            Assert.IsInstanceOfType(typeof(LiteralLayoutRenderer), lr.FileName.Renderers[0]);
            Assert.IsInstanceOfType(typeof(FileContentsLayoutRenderer), lr.FileName.Renderers[1]);
            Assert.IsInstanceOfType(typeof(LiteralLayoutRenderer), lr.FileName.Renderers[2]);

            LiteralLayoutRenderer lr1 = (LiteralLayoutRenderer)lr.FileName.Renderers[0];
            FileContentsLayoutRenderer fc = (FileContentsLayoutRenderer)lr.FileName.Renderers[1];
            LiteralLayoutRenderer lr2 = (LiteralLayoutRenderer)lr.FileName.Renderers[2];

            Assert.AreEqual("${basedir}/aaa.txt", fc.FileName.Text);

        }
    }
}
