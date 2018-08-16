using NUnit.Framework;
using SearchEngine.MemoryStore;

namespace SearchEngine.Tests
{
    [TestFixture]
    public class FieldStoreTests
    {
        [Test]
        public void ShouldReturnNullIfDoesNotExist()
        {
            var store = new FieldStore();

            var docId = Utils.RandomInteger();

            Assert.IsNull(store.GetValue(docId, Utils.RandomString()));

            store.SetValue(docId, Utils.RandomString(), Utils.RandomString());

            Assert.IsNull(store.GetValue(5, Utils.RandomString()));
        }

        [Test]
        public void ShouldReturnValue()
        {
            var store = new FieldStore();

            var docId = Utils.RandomInteger();
            var fieldName = Utils.RandomString();
            var value = Utils.RandomString();

            store.SetValue(docId, fieldName, value);

            Assert.AreEqual(value, store.GetValue(docId, fieldName));
        }

        [Test]
        public void ShouldChangeValue()
        {
            var store = new FieldStore();

            var docId = Utils.RandomInteger();
            var fieldName = Utils.RandomString();
            var value = Utils.RandomString();

            store.SetValue(docId, fieldName, value);

            var value2 = Utils.RandomString();
            store.SetValue(docId, fieldName, value2);

            Assert.AreEqual(value2, store.GetValue(docId, fieldName));
        }
    }
}
