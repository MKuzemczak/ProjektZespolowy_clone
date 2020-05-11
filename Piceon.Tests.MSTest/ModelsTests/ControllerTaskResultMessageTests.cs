using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Piceon.Models;

namespace Piceon.Tests.MSTest.ModelsTests
{
    [TestClass]
    public class ControllerTaskResultMessageTests
    {
        [TestMethod]
        public void FromJsonTest()
        {
            string msg = "{\"taskid\":1,\"result\":\"RESULT\",\"error_message\":\"ERROR\",\"images\":[[\"image1\",\"image2\"],[\"image3\",\"image4\"]]}";
            var expected = new ControllerTaskResultMessage()
            { 
                taskid = 1,
                result = "RESULT",
                error_message = "ERROR",
                images = 
                {
                    new List<int>() {1, 2},
                    new List<int>() {3, 4}
                }
            };
            var result = ControllerTaskResultMessage.FromJson(msg);
            Assert.AreEqual(expected.taskid, result.taskid);
            Assert.AreEqual(expected.result, result.result);
            Assert.AreEqual(expected.error_message, result.error_message);
            Assert.IsTrue(expected.images.SequenceEqual(result.images));
        }
    }
}
