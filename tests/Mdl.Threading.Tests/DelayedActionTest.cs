namespace Mdl.Threading.Tests;

using System;
using System.Threading.Tasks;
using Xunit;

public class DelayedActionTest
{
    [Fact]
    public void Delay_WhenInit_AsADefaultValue()
    {
        DelayedAction sut = new(_ => { });

        Assert.NotEqual(TimeSpan.FromSeconds(0), sut.Delay);
    }

    [Fact]
    public void Delay_WhenSet_ReturnValue()
    {
        DelayedAction sut = new(_ => { })
        {
            Delay = TimeSpan.FromSeconds(2),
        };

        Assert.Equal(TimeSpan.FromSeconds(2), sut.Delay);
    }

    [Fact]
    public void Start_WhenCalledMultipleTimes_ShouldRunActionOneTime()
    {
        int n = 0;

        DelayedAction sut = new(_ => n++)
        {
            Delay = TimeSpan.FromSeconds(1),
        };

        sut.Start();
        sut.Start();
        sut.Start();
        sut.Start();
        sut.Start();

        Assert.Equal(0, n);

        Task.Delay(TimeSpan.FromSeconds(7)).Wait();

        Assert.Equal(1, n);
    }

    [Fact]
    public void Stop_WhenCalled_ActionShouldNotBeExecuted()
    {
        int n = 0;

        DelayedAction sut = new(_ => n++)
        {
            Delay = TimeSpan.FromSeconds(1),
        };

        sut.Start();
        sut.Start();
        sut.Stop();

        Assert.Equal(0, n);

        Task.Delay(TimeSpan.FromSeconds(7)).Wait();

        Assert.Equal(0, n);
    }

    [Fact]
    public void Start_WhenCalledMultipleTimesWithoutOverlap_ActionShouldBeExecutedSeveralTimes()
    {
        int n = 0;

        DelayedAction sut = new(_ => n++)
        {
            Delay = TimeSpan.FromSeconds(1),
        };

        sut.Start();
        Task.Delay(TimeSpan.FromSeconds(2)).Wait();

        sut.Start();
        Task.Delay(TimeSpan.FromSeconds(2)).Wait();

        sut.Start();
        Task.Delay(TimeSpan.FromSeconds(2)).Wait();

        Assert.Equal(3, n);
    }

    [Fact]
    public void Stop_WhenCalledDuringAction_ActionShouldReceiveCancellation()
    {
        int n = 0;

        DelayedAction sut = new(cancellationToken =>
        {
            Task.Delay(TimeSpan.FromSeconds(2), cancellationToken).Wait(cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            n++;
        })
        {
            Delay = TimeSpan.FromSeconds(1),
        };

        sut.Start();
        Task.Delay(TimeSpan.FromSeconds(2)).Wait();

        sut.Stop();
        Task.Delay(TimeSpan.FromSeconds(4)).Wait();

        Assert.Equal(0, n);
    }
}
