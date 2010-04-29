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
using NLog.Targets;
using NLog.Targets.Wrappers;
using NLog.Targets.Compound;

namespace NLog.UnitTests
{
    [TestFixture]
	public class TargetConfigurationTests : NLogTestBase
	{
        [Test]
        public void SimpleTest()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(@"
            <nlog>
                <targets>
                    <target name='d' type='Debug' layout='${message}' />
                </targets>
            </nlog>");

            LoggingConfiguration c = new XmlLoggingConfiguration(doc.DocumentElement, null);
            DebugTarget t = c.FindTargetByName("d") as DebugTarget;
            Assert.IsNotNull(t);
            Assert.AreEqual(t.Name, "d");
            Assert.AreEqual("${message}", t.Layout);
            Layout l = t.CompiledLayout as Layout;
            Assert.IsNotNull(l);
            Assert.AreEqual(1, l.Renderers.Length);
            Assert.IsInstanceOfType(typeof(MessageLayoutRenderer), l.Renderers[0]);
        }

        [Test]
        public void SimpleTest2()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(@"
            <nlog>
                <targets>
                    <target name='d' type='Debug' layout='${message:padding=10} ${level}' />
                </targets>
            </nlog>");

            LoggingConfiguration c = new XmlLoggingConfiguration(doc.DocumentElement, null);
            DebugTarget t = c.FindTargetByName("d") as DebugTarget;
            Assert.IsNotNull(t);
            Assert.AreEqual(t.Name, "d");
            Assert.AreEqual("${message:padding=10} ${level}", t.Layout);
            Layout l = t.CompiledLayout as Layout;
            Assert.IsNotNull(l);
            Assert.AreEqual(3, l.Renderers.Length);
            Assert.IsInstanceOfType(typeof(MessageLayoutRenderer), l.Renderers[0]);
            Assert.IsInstanceOfType(typeof(LiteralLayoutRenderer), l.Renderers[1]);
            Assert.IsInstanceOfType(typeof(LevelLayoutRenderer), l.Renderers[2]);
            Assert.AreEqual(10, l.Renderers[0].Padding);
        }

        [Test]
        public void WrapperTest()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(@"
            <nlog>
                <targets>
                    <target name='b' type='BufferingWrapper' bufferSize='19'>
                        <target name='a' type='AsyncWrapper'>
                            <target name='c' type='Debug' layout='${message}' />
                        </target>
                    </target>
                </targets>
            </nlog>");

            LoggingConfiguration c = new XmlLoggingConfiguration(doc.DocumentElement, null);
            Assert.IsNotNull(c.FindTargetByName("a"));
            Assert.IsNotNull(c.FindTargetByName("b"));
            Assert.IsNotNull(c.FindTargetByName("c"));

            Assert.IsInstanceOfType(typeof(BufferingTargetWrapper), c.FindTargetByName("b"));
            Assert.IsInstanceOfType(typeof(AsyncTargetWrapper), c.FindTargetByName("a"));
            Assert.IsInstanceOfType(typeof(DebugTarget), c.FindTargetByName("c"));

            BufferingTargetWrapper btw = c.FindTargetByName("b") as BufferingTargetWrapper;
            AsyncTargetWrapper atw = c.FindTargetByName("a") as AsyncTargetWrapper;
            DebugTarget dt = c.FindTargetByName("c") as DebugTarget;

            Assert.AreSame(atw, btw.WrappedTarget);
            Assert.AreSame(dt, atw.WrappedTarget);
            Assert.AreEqual(19, btw.BufferSize);
        }

        [Test]
        public void CompoundTest()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(@"
            <nlog>
                <targets>
                    <target name='rr' type='RoundRobinGroup'>
                        <target name='d1' type='Debug' layout='${message}1' />
                        <target name='d2' type='Debug' layout='${message}2' />
                        <target name='d3' type='Debug' layout='${message}3' />
                        <target name='d4' type='Debug' layout='${message}4' />
                    </target>
                </targets>
            </nlog>");

            LoggingConfiguration c = new XmlLoggingConfiguration(doc.DocumentElement, null);

            Assert.IsNotNull(c.FindTargetByName("rr"));
            Assert.IsNotNull(c.FindTargetByName("d1"));
            Assert.IsNotNull(c.FindTargetByName("d2"));
            Assert.IsNotNull(c.FindTargetByName("d3"));
            Assert.IsNotNull(c.FindTargetByName("d4"));

            Assert.IsInstanceOfType(typeof(RoundRobinTarget), c.FindTargetByName("rr"));
            Assert.IsInstanceOfType(typeof(DebugTarget), c.FindTargetByName("d1"));
            Assert.IsInstanceOfType(typeof(DebugTarget), c.FindTargetByName("d2"));
            Assert.IsInstanceOfType(typeof(DebugTarget), c.FindTargetByName("d3"));
            Assert.IsInstanceOfType(typeof(DebugTarget), c.FindTargetByName("d4"));

            RoundRobinTarget rr = c.FindTargetByName("rr") as RoundRobinTarget;
            DebugTarget d1 = c.FindTargetByName("d1") as DebugTarget;
            DebugTarget d2 = c.FindTargetByName("d2") as DebugTarget;
            DebugTarget d3 = c.FindTargetByName("d3") as DebugTarget;
            DebugTarget d4 = c.FindTargetByName("d4") as DebugTarget;

            Assert.AreEqual(4, rr.Targets.Count);
            Assert.AreSame(d1, rr.Targets[0]);
            Assert.AreSame(d2, rr.Targets[1]);
            Assert.AreSame(d3, rr.Targets[2]);
            Assert.AreSame(d4, rr.Targets[3]);

            Assert.AreEqual(d1.Layout, "${message}1");
            Assert.AreEqual(d2.Layout, "${message}2");
            Assert.AreEqual(d3.Layout, "${message}3");
            Assert.AreEqual(d4.Layout, "${message}4");
        }
    }
}
