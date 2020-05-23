using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Animation;

namespace XpoMusic.Helpers
{
    public static class StoryboardExtensions
    {
        public static async Task RunAsync(this Storyboard storyboard)
        {
            TaskCompletionSource<bool> tcs;
            tcs = new TaskCompletionSource<bool>();

            void handler(object s, object e) => tcs.TrySetResult(true);

            storyboard.Completed += handler;

            storyboard.Begin();
            await tcs.Task;

            storyboard.Completed -= handler;
        }
    }
}
