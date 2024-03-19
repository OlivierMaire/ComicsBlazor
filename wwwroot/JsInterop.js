// This is a JavaScript module that is loaded on demand. It can export any number of
// functions, and may import other JavaScript modules if required.

export function showPrompt(message) {
  return prompt(message, 'Type anything here');
}

export var comic = {
  dotNetReference: undefined,
  init(pDotNetReference) {
    console.log("initialize interop...");
    this.dotNetReference = pDotNetReference;
    window.addEventListener("resize", this.reportWindowSize.bind(this));
    this.reportWindowSize(null);
    this.addKeyboardListenerEvent(this);
  },
  reportWindowSize(e) {
    console.log("reportWindowSize");
    if (this.dotNetReference) {
      this.dotNetReference.invokeMethodAsync('OnResizeFromJs',
        window.innerWidth, window.innerHeight);
    }

  },
  addKeyboardListenerEvent: function (me) {
    let serializeEvent = function (e) {
      if (e) {
        return {
          key: e.key,
          code: e.keyCode.toString(),
          location: e.location,
          repeat: e.repeat,
          ctrlKey: e.ctrlKey,
          shiftKey: e.shiftKey,
          altKey: e.altKey,
          metaKey: e.metaKey,
          type: e.type
        };
      }
    };

    window.document.addEventListener('keydown', function (e) {
      console.log(`KEYDOWN: M ${e.metaKey} ${e.key}`);
      me.dotNetReference.invokeMethodAsync('JsKeyDown',
        serializeEvent(e));
    });
  }
}