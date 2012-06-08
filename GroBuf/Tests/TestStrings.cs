using NUnit.Framework;

namespace GroBuf.Tests
{
    [TestFixture]
    public class TestStrings
    {
        [SetUp]
        public void SetUp()
        {
            serializer = new Serializer();
        }

        [Test]
        public void TestString()
        {
            const string s = "zzz ������� \u2376 \uDEAD";
            byte[] bytes = serializer.Serialize(s);
            var deserialize = serializer.Deserialize<string>(bytes);
            Assert.AreEqual("zzz ������� \u2376 \uDEAD", deserialize);
        }

        [Test]
        public void TestStringInProp()
        {
            const string s = "zzz ������� \u2376 \uDEAD";
            byte[] bytes = serializer.Serialize(new WithS {S = s});
            var deserialize = serializer.Deserialize<WithS>(bytes);
            Assert.AreEqual("zzz ������� \u2376 \uDEAD", deserialize.S);
        }

        [Test]
        public void TestStringNull()
        {
            byte[] bytes = serializer.Serialize<string>(null);
            var deserialize = serializer.Deserialize<string>(bytes);
            Assert.AreEqual("", deserialize);
        }

        [Test]
        public void TestStringNullInProp()
        {
            byte[] bytes = serializer.Serialize(new WithS());
            var deserialize = serializer.Deserialize<WithS>(bytes);
            Assert.AreEqual(null, deserialize.S);
        }

        [Test]
        public void TestStringEmpty()
        {
            byte[] bytes = serializer.Serialize("");
            var deserialize = serializer.Deserialize<string>(bytes);
            Assert.AreEqual("", deserialize);
        }

        [Test]
        public void TestStringEmptyInProp()
        {
            byte[] bytes = serializer.Serialize(new WithS {S = ""});
            var deserialize = serializer.Deserialize<WithS>(bytes);
            Assert.AreEqual("", deserialize.S);
        }

        [Test]
        public void TestStringEmptyInArrayProp()
        {
            byte[] bytes = serializer.Serialize(new WithS {Strings = new[] {"", null, "zzz"}});
            var deserialize = serializer.Deserialize<WithS>(bytes);
            Assert.IsNotNull(deserialize);
            Assert.IsNotNull(deserialize.Strings);
            CollectionAssert.AreEqual(new[] {"", null, "zzz"}, deserialize.Strings);
        }

        [Test]
        public void TestStringEmptyInArray()
        {
            byte[] bytes = serializer.Serialize(new[] {"", null, "zzz"});
            var deserialize = serializer.Deserialize<string[]>(bytes);
            Assert.IsNotNull(deserialize);
            CollectionAssert.AreEqual(new[] {"", null, "zzz"}, deserialize);
        }

        public class WithS
        {
            public string S { get; set; }

            public string[] Strings { get; set; }
        }

        private Serializer serializer;
    }
}