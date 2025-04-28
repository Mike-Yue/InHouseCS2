namespace InHouseCS2.Core.Common
{
    public abstract class MockFixture<T>
    {
        public abstract T SetSubject();

        public abstract void VerifyAll();

        public async Task TestComponentAndVerifyMocksAsync(Func<T, Task> testActionAsync)
        {
            var subject = this.SetSubject();
            await testActionAsync(subject);
            this.VerifyAll();
        }

        public void TestComponentAndVerifyMocks(Action<T> testAction)
        {
            var subject = this.SetSubject();
            testAction(subject);
            this.VerifyAll();
        }
    }
}
