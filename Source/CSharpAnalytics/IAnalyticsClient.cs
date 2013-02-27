using CSharpAnalytics.Activities;

namespace CSharpAnalytics
{
    public interface IAnalyticsClient
    {
        void Track(IActivity activity);
    }
}