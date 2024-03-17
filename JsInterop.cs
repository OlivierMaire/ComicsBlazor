using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace ComicsBlazor;

// This class provides an example of how JavaScript functionality can be wrapped
// in a .NET class for easy consumption. The associated JavaScript module is
// loaded on demand when first needed.
//
// This class can be registered as scoped DI service and then injected into Blazor
// components for use.

public class ComicsJsInterop : IAsyncDisposable
{
    public Action<Size>? OnWindowResized { get; set; }
    public Action<string>? OnKeyDown { get; set; }

    private readonly Lazy<Task<IJSObjectReference>> moduleTask;

    // [DynamicDependency(nameof(UpdateCurrentTimestampFromJs))]
    public ComicsJsInterop(IJSRuntime jsRuntime)
    {
        moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/ComicsBlazor/JsInterop.js").AsTask());
    }

    public async ValueTask<string> Prompt(string message)
    {
        var module = await moduleTask.Value;
        return await module.InvokeAsync<string>("showPrompt", message);
    }

    public async ValueTask Init()
    {
        var module = await moduleTask.Value;
        var lDotNetReference = DotNetObjectReference.Create(this);
        await module.InvokeVoidAsync("comic.init", lDotNetReference);
    }

    public async ValueTask DisposeAsync()
    {
        if (moduleTask.IsValueCreated)
        {
            var module = await moduleTask.Value;
            await module.DisposeAsync();
        }
    }

    [JSInvokable("OnResizeFromJs")]
    public void OnResizeFromJs(int width, int height)
    {
        OnWindowResized?.Invoke(new Size(width, height));
    }

    [JSInvokable("JsKeyDown")]
    public void JsKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "ArrowLeft" || e.Key == "ArrowRight")
            OnKeyDown?.Invoke(e.Key);

    }
}
