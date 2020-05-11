using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Piceon.Models;
using Windows.UI.Core;

namespace Piceon.Tests.MSTest.ModelsTests
{
    [TestClass]
    public class ControllerTaskRequestMessageTests
    {
        [TestMethod]
        public void ToJsonTest()
        {
            ControllerTaskRequestMessage msg = new ControllerTaskRequestMessage()
            {
                taskid = 1,
                type = 2
            };
            msg.images.Add("image/1");
            msg.images.Add("image/2");

            string result = msg.ToJson();
            string expected = "{\"taskid\":1,\"type\":2,\"images\":[\"image/1\",\"image/2\"]}";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void FromJsonTest()
        {
            string json = "{\"taskid\":1,\"type\":2,\"images\":[\"image/1\",\"image/2\"]}";
            ControllerTaskRequestMessage expected = new ControllerTaskRequestMessage()
            {
                taskid = 1,
                type = 2
            };
            expected.images.Add("image/1");
            expected.images.Add("image/2");

            var result = ControllerTaskRequestMessage.FromJson(json);

            Assert.AreEqual(expected.taskid, result.taskid);
            Assert.AreEqual(expected.type, result.type);
            Assert.IsTrue(expected.images.SequenceEqual(result.images));
        }
    }
}
